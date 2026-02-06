using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using mu88.Shared.OpenTelemetry;
using PasswordTrainer;

if (args.Contains("initialize-secrets", StringComparer.OrdinalIgnoreCase))
{
    var cliAppBuilder = Host.CreateApplicationBuilder(args);
    cliAppBuilder.Services
                 .AddHostedService<SecretInitializationWorker>()
                 .AddOptions<PasswordTrainerOptions>()
                 .Bind(cliAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName))
                 .Validate(options => Directory.Exists(options.DataPath), "DataPath must exist")
                 .Validate(options => Directory.Exists(options.SecretsPath), "SecretsPath must exist")
                 .ValidateDataAnnotations()
                 .ValidateOnStart();
    cliAppBuilder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning); // reduce logging noise
    await cliAppBuilder.Build().RunAsync();
    return;
}

var webAppBuilder = WebApplication.CreateBuilder();

webAppBuilder.Services.ConfigureOpenTelemetry("passwordtrainer", webAppBuilder.Configuration);
webAppBuilder.Services
             .AddOptions<PasswordTrainerOptions>()
             .Bind(webAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName))
             .Validate(options => Directory.Exists(options.DataPath), "DataPath must exist")
             .Validate(options => Directory.Exists(options.SecretsPath), "SecretsPath must exist")
             .Validate(options => File.Exists(options.GetPepperFilePath()), "Pepper file must exist")
             .Validate(options => File.Exists(options.GetPinHashFilePath()), "PIN hash file must exist")
             .Validate(options => File.Exists(options.GetSecretsFilePath()), "Secrets file must exist")
             .ValidateDataAnnotations()
             .ValidateOnStart();
webAppBuilder.Services.AddHealthChecks();

var passwordTrainerOptions = new PasswordTrainerOptions();
webAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName).Bind(passwordTrainerOptions);
webAppBuilder.Services.AddDataProtection()
             .PersistKeysToFileSystem(new DirectoryInfo(passwordTrainerOptions.DataPath))
             .SetApplicationName("PasswordTrainer");

webAppBuilder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("check-endpoint",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = passwordTrainerOptions.RateLimitingPermitLimit,
                    Window = TimeSpan.FromMinutes(passwordTrainerOptions.RateLimitingWindowMinutes),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
});

var app = webAppBuilder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

app.UseRateLimiter();

if (!string.IsNullOrWhiteSpace(passwordTrainerOptions.PathBase))
{
    app.UsePathBase(passwordTrainerOptions.PathBase);
}

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseRouting();

app.MapHealthChecks("/healthz");

app.MapPost("/check",
       async (CheckRequest request, IDataProtectionProvider dp, IOptions<PasswordTrainerOptions> options, CancellationToken cancellationToken) =>
       {
           var validationContext = new ValidationContext(request);
           var validationResults = new List<ValidationResult>();
           if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
           {
               return Results.BadRequest(validationResults.Select(r => r.ErrorMessage));
           }

           var pepper = await File.ReadAllBytesAsync(options.Value.GetPepperFilePath(), cancellationToken);
           var expectedPinHash = await File.ReadAllTextAsync(options.Value.GetPinHashFilePath(), cancellationToken);
           var pinBytes = Encoding.UTF8.GetBytes(request.Pin);
           bool pinValid;
           try
           {
               pinValid = Argon2.Verify(expectedPinHash, pinBytes, pepper);
           }
           finally
           {
               Array.Clear(pinBytes, 0, pinBytes.Length);
           }

           if (!pinValid)
           {
               return Results.BadRequest("Invalid credentials");
           }

           string decrypted;
           try
           {
               var protector = dp.CreateProtector("pw-store");
               decrypted = protector.Unprotect(await File.ReadAllTextAsync(options.Value.GetSecretsFilePath(), cancellationToken));
           }
           catch
           {
               return Results.Problem("Server error");
           }

           var store = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted) ?? new Dictionary<string, string>(StringComparer.Ordinal);
           if (!store.TryGetValue(request.Id, out var expectedPasswordHash))
           {
               return Results.BadRequest("Invalid credentials");
           }

           var passwordBytes = Array.Empty<byte>();
           bool passwordValid;
           try
           {
               passwordBytes = Convert.FromBase64String(request.Password);
               passwordValid = Argon2.Verify(expectedPasswordHash, passwordBytes, pepper);
           }
           finally
           {
               Array.Clear(passwordBytes, 0, passwordBytes.Length);
           }

           return !passwordValid ? Results.BadRequest("Invalid credentials") : Results.Ok();
       })
   .RequireRateLimiting("check-endpoint");

await app.RunAsync();