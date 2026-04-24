using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace PasswordTrainer;

internal sealed class SecretInitializationWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly PasswordTrainerOptions _passwordTrainerOptions;
    private readonly IConsole _console;
    private readonly IFile _file;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public SecretInitializationWorker(
        IOptions<PasswordTrainerOptions> options,
        IHostApplicationLifetime hostApplicationLifetime,
        IConsole console,
        IFile file,
        IDataProtectionProvider dataProtectionProvider)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _passwordTrainerOptions = options.Value;
        _console = console;
        _file = file;
        _dataProtectionProvider = dataProtectionProvider;
    }

    [SuppressMessage("Design", "MA0076:Do not use implicit culture-sensitive ToString in interpolated strings", Justification = "Sequential display index – culture-invariant formatting not required")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _console.WriteLine("=== PasswordTrainer Init Mode ===");

        _console.Write("Enter new App-PIN: ");
        var pin = ReadSecretFromConsole();
        if (string.IsNullOrWhiteSpace(pin))
        {
            throw new InvalidOperationException("App PIN must not be empty");
        }

        var pepperFile = _passwordTrainerOptions.GetPepperFilePath();
        byte[] pepper;
        if (_file.Exists(pepperFile))
        {
            pepper = await _file.ReadAllBytesAsync(pepperFile, stoppingToken);
        }
        else
        {
            pepper = RandomNumberGenerator.GetBytes(32);
            await _file.WriteAllBytesAsync(pepperFile, pepper, stoppingToken);
        }

        var pinHash = HashWithClear(Encoding.UTF8.GetBytes(pin), pepper);
        await _file.WriteAllTextAsync(_passwordTrainerOptions.GetPinHashFilePath(), pinHash, stoppingToken);

        var passwordDict = new Dictionary<string, string>(StringComparer.Ordinal);
        _console.Write("How many passwords? ");
        if (!int.TryParse(_console.ReadLine(), CultureInfo.InvariantCulture, out var count))
        {
            throw new InvalidOperationException("Please enter a valid number.");
        }

        for (var i = 0; i < count; ++i)
        {
            _console.Write($"ID #{i + 1}: ");
            var id = _console.ReadLine() ?? throw new InvalidOperationException("ID must not be empty");
            _console.Write($"Password for '{id}': ");
            var password = ReadSecretFromConsole();
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Password must not be empty");
            }

            passwordDict[id] = HashWithClear(Encoding.UTF8.GetBytes(password), pepper);
        }

        var protector = _dataProtectionProvider.CreateProtector("pw-store");
        var encrypted = protector.Protect(JsonSerializer.Serialize(passwordDict));
        await _file.WriteAllTextAsync(_passwordTrainerOptions.GetSecretsFilePath(), encrypted, stoppingToken);

        _console.WriteLine("=== Init Complete ===");

        _hostApplicationLifetime.StopApplication();
    }

    private static string HashWithClear(byte[] bytes, byte[] pepper)
    {
        // Stryker disable once Block : the try/finally wrapper is security hygiene; removing it would skip Array.Clear which is not observable via the return value
        try
        {
            return Argon2.Hash(bytes, pepper);
        }
        finally
        {
            // Stryker disable once Statement : clearing the passed byte array is security hygiene; not observable from outside the method
            Array.Clear(bytes, 0, bytes.Length);
        }
    }

    private string ReadSecretFromConsole()
    {
        var sb = new StringBuilder();
        while (true)
        {
            var key = _console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                _console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length <= 0)
                {
                    continue;
                }

                sb.Length--;
                _console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                _console.Write("*");
            }
        }

        return sb.ToString();
    }
}
