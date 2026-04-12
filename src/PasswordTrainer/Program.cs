using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using mu88.Shared.OpenTelemetry;
using PasswordTrainer;

if (args.Contains("initialize-secrets", StringComparer.OrdinalIgnoreCase))
{
    await CliBootstrap.RunAsync(args);
    return;
}

var webAppBuilder = WebApplication.CreateBuilder();

webAppBuilder.Services.ConfigureOpenTelemetry("passwordtrainer", webAppBuilder.Configuration);
webAppBuilder.Services
    .AddOptions<PasswordTrainerOptions>()
    .Bind(webAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName))
    .Validate(opts => Directory.Exists(opts.DataPath), "DataPath must exist")
    .Validate(opts => Directory.Exists(opts.SecretsPath), "SecretsPath must exist")
    .Validate(opts => File.Exists(opts.GetPepperFilePath()), "Pepper file must exist")
    .Validate(opts => File.Exists(opts.GetPinHashFilePath()), "PIN hash file must exist")
    .Validate(opts => File.Exists(opts.GetSecretsFilePath()), "Secrets file must exist")
    .ValidateDataAnnotations()
    .ValidateOnStart();
webAppBuilder.Services.AddHealthChecks();
webAppBuilder.Services.AddSingleton<IFile, SystemFile>();
webAppBuilder.Services.AddScoped<PasswordCheckService>();
webAppBuilder.Services.AddDataProtection().SetApplicationName("PasswordTrainer");
webAppBuilder.Services.AddOptions<KeyManagementOptions>()
    .Configure<IOptions<PasswordTrainerOptions>>((kmo, trainerOptions) =>
        kmo.XmlRepository = new FileSystemXmlRepository(
            new DirectoryInfo(trainerOptions.Value.DataPath), NullLoggerFactory.Instance));
webAppBuilder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    rateLimiterOptions.AddPolicy("check-endpoint",
        context =>
        {
            var opts = context.RequestServices
                .GetRequiredService<IOptionsMonitor<PasswordTrainerOptions>>().CurrentValue;
            return RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = opts.RateLimitingPermitLimit,
                    Window = TimeSpan.FromMinutes(opts.RateLimitingWindowMinutes),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        });
});

var app = webAppBuilder.Build();

// UsePathBase must run before routing so subsequent middleware sees the stripped path.
// Resolved post-Build() so WAF-injected configuration is included.
var resolvedOptions = app.Services.GetRequiredService<IOptions<PasswordTrainerOptions>>().Value;
if (!string.IsNullOrWhiteSpace(resolvedOptions.PathBase))
{
    app.UsePathBase(resolvedOptions.PathBase);
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self'";
    await next();
});
app.UseDefaultFiles();
app.MapStaticAssets();
app.UseRouting();

// UseRateLimiter must run after UseRouting so endpoint metadata (RequireRateLimiting) is available.
app.UseRateLimiter();

app.MapHealthChecks("/healthz");
app.MapPost("/check",
    (CheckRequest request, PasswordCheckService svc, CancellationToken ct) =>
        svc.CheckAsync(request, ct))
    .RequireRateLimiting("check-endpoint");

await app.RunAsync();
