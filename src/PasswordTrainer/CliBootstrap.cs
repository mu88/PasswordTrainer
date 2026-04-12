using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using PasswordTrainer;

namespace PasswordTrainer;

[ExcludeFromCodeCoverage(Justification = "CLI host bootstrap — SecretInitializationWorker is fully covered by unit tests")]
internal static class CliBootstrap
{
    internal static async Task RunAsync(string[] args)
    {
        var cliAppBuilder = Host.CreateApplicationBuilder(args);

        var cliOptions = new PasswordTrainerOptions();
        cliAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName).Bind(cliOptions);

        cliAppBuilder.Services
            .AddSingleton<IConsole, SystemConsole>()
            .AddSingleton<IFile, SystemFile>()
            .AddHostedService<SecretInitializationWorker>()
            .AddOptions<PasswordTrainerOptions>()
            .Bind(cliAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName))
            .Validate(opts => Directory.Exists(opts.DataPath), "DataPath must exist")
            .Validate(opts => Directory.Exists(opts.SecretsPath), "SecretsPath must exist")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        cliAppBuilder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(cliOptions.DataPath))
            .SetApplicationName("PasswordTrainer");
        cliAppBuilder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

        await cliAppBuilder.Build().RunAsync();
    }
}
