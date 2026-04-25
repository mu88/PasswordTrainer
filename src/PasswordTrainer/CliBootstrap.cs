using System.Diagnostics.CodeAnalysis;
using PasswordTrainer;

namespace PasswordTrainer;

[ExcludeFromCodeCoverage(Justification = "CLI host bootstrap — SecretInitializationWorker is fully covered by unit tests")]
internal static class CliBootstrap
{
    internal static async Task RunAsync(string[] args)
    {
        var cliAppBuilder = Host.CreateApplicationBuilder(args);

        cliAppBuilder.Services
            .AddSingleton<IConsole, SystemConsole>()
            .AddSingleton<IFile, SystemFile>()
            .AddSingleton<ISecretsEncryption, AesGcmSecretsEncryption>()
            .AddHostedService<SecretInitializationWorker>()
            .AddOptions<PasswordTrainerOptions>()
            .Bind(cliAppBuilder.Configuration.GetSection(PasswordTrainerOptions.SectionName))
            .Validate(opts => Directory.Exists(opts.DataPath), "DataPath must exist")
            .Validate(opts => Directory.Exists(opts.SecretsPath), "SecretsPath must exist")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        cliAppBuilder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

        await cliAppBuilder.Build().RunAsync();
    }
}
