using System.Globalization;
using System.Text;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace PasswordTrainer;

public class SecretInitializationWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly PasswordTrainerOptions _passwordTrainerOptions;

    public SecretInitializationWorker(
        IOptions<PasswordTrainerOptions> options,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _passwordTrainerOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("=== PasswordTrainer Init Mode ===");

        Console.WriteLine("Enter new App-PIN:");
        var pin = Console.ReadLine() ?? throw new InvalidOperationException("App PIN must not be empty");

        var pepperFile = _passwordTrainerOptions.GetPepperFilePath();
        var pepper = File.Exists(pepperFile) ? await File.ReadAllBytesAsync(pepperFile, stoppingToken) : Guid.NewGuid().ToByteArray();
        if (!File.Exists(pepperFile))
        {
            await File.WriteAllBytesAsync(pepperFile, pepper, stoppingToken);
        }

        var pinHash = Argon2.Hash(Encoding.UTF8.GetBytes(pin), pepper);
        await File.WriteAllTextAsync(_passwordTrainerOptions.GetPinHashFilePath(), pinHash, stoppingToken);

        var passwordDict = new Dictionary<string, string>(StringComparer.Ordinal);
        Console.Write("How many passwords? ");
        int.TryParse(Console.ReadLine(), CultureInfo.InvariantCulture, out var count);
        for (var i = 0; i < count; ++i)
        {
            Console.Write($"ID #{i + 1}: ");
            var id = Console.ReadLine() ?? throw new InvalidOperationException("ID must not be empty");
            Console.WriteLine($"Password for '{id}': ");
            var password = Console.ReadLine() ?? throw new InvalidOperationException("Password must not be empty");
            passwordDict[id] = Argon2.Hash(Encoding.UTF8.GetBytes(password), pepper);
        }

        IDataProtectionProvider dataProtection = DataProtectionProvider.Create(
            new DirectoryInfo(_passwordTrainerOptions.DataPath),
            cfg => cfg.SetApplicationName("PasswordTrainer"));
        IDataProtector protector = dataProtection.CreateProtector("pw-store");
        var encrypted = protector.Protect(JsonSerializer.Serialize(passwordDict));
        await File.WriteAllTextAsync(_passwordTrainerOptions.GetSecretsFilePath(), encrypted, stoppingToken);

        Console.WriteLine("=== Init Complete ===");

        _hostApplicationLifetime.StopApplication();
    }
}