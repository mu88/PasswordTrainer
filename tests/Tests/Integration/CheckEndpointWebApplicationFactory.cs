using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using PasswordTrainer;

namespace Tests.Integration;

internal sealed class CheckEndpointWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dataPath;
    private readonly int _rateLimitingPermitLimit;
    private readonly string? _pathBase;

    public CheckEndpointWebApplicationFactory(string? dataPath = null, int rateLimitingPermitLimit = 15, string? pathBase = null)
    {
        _dataPath = dataPath ?? Path.Combine(AppContext.BaseDirectory, "testData", "data");
        _rateLimitingPermitLimit = rateLimitingPermitLimit;
        _pathBase = pathBase;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // MapStaticAssets() resolves the manifest file using IWebHostEnvironment.ApplicationName,
        // which is set from the test runner entry assembly ("testhost") in the WAF context.
        // Overriding it here (before Build() is called) makes ASP.NET Core find the correct
        // PasswordTrainer.staticwebassets.endpoints.json in the test output directory.
        builder.ConfigureServices((context, _) =>
            context.HostingEnvironment.ApplicationName = typeof(Program).Assembly.GetName().Name!);

        var config = new Dictionary<string, string?>
        {
            [$"{PasswordTrainerOptions.SectionName}:DataPath"] = _dataPath,
            [$"{PasswordTrainerOptions.SectionName}:SecretsPath"] =
                Path.Combine(AppContext.BaseDirectory, "testData", "secrets"),
            [$"{PasswordTrainerOptions.SectionName}:RateLimitingPermitLimit"] =
                _rateLimitingPermitLimit.ToString(),
        };

        if (_pathBase is not null)
        {
            config[$"{PasswordTrainerOptions.SectionName}:PathBase"] = _pathBase;
        }

        builder.ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(config));
    }
}
