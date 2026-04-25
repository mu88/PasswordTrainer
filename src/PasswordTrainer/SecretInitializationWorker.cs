using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Options;

namespace PasswordTrainer;

internal sealed class SecretInitializationWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly PasswordTrainerOptions _passwordTrainerOptions;
    private readonly IConsole _console;
    private readonly IFile _file;
    private readonly ISecretsEncryption _secretsEncryption;

    public SecretInitializationWorker(
        IOptions<PasswordTrainerOptions> options,
        IHostApplicationLifetime hostApplicationLifetime,
        IConsole console,
        IFile file,
        ISecretsEncryption secretsEncryption)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _passwordTrainerOptions = options.Value;
        _console = console;
        _file = file;
        _secretsEncryption = secretsEncryption;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _console.WriteLine("=== PasswordTrainer Init Mode ===");

        _console.Write("Enter new App-PIN: ");
        var pin = ReadSecretFromConsole();
        if (pin is null)
        {
            _hostApplicationLifetime.StopApplication();
            return;
        }

        if (string.IsNullOrWhiteSpace(pin))
        {
            throw new InvalidOperationException("App PIN must not be empty");
        }

        var pepper = await LoadOrCreatePepperAsync(stoppingToken);
        var pinHash = HashWithClear(Encoding.UTF8.GetBytes(pin), pepper);
        await _file.WriteAllTextAsync(_passwordTrainerOptions.GetPinHashFilePath(), pinHash, stoppingToken);

        var passwordDict = ReadPasswords(pepper);
        if (passwordDict is null)
        {
            return;
        }

        var encrypted = _secretsEncryption.Encrypt(pepper, JsonSerializer.Serialize(passwordDict));
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

    private async Task<byte[]> LoadOrCreatePepperAsync(CancellationToken cancellationToken)
    {
        var pepperFile = _passwordTrainerOptions.GetPepperFilePath();
        if (_file.Exists(pepperFile))
        {
            return await _file.ReadAllBytesAsync(pepperFile, cancellationToken);
        }

        var pepper = RandomNumberGenerator.GetBytes(32);
        await _file.WriteAllBytesAsync(pepperFile, pepper, cancellationToken);
        return pepper;
    }

    [SuppressMessage("Design", "MA0076:Do not use implicit culture-sensitive ToString in interpolated strings", Justification = "Sequential display index – culture-invariant formatting not required")]
    private Dictionary<string, string>? ReadPasswords(byte[] pepper)
    {
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
            if (password is null)
            {
                _hostApplicationLifetime.StopApplication();
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Password must not be empty");
            }

            passwordDict[id] = HashWithClear(Encoding.UTF8.GetBytes(password), pepper);
        }

        return passwordDict;
    }

    private string? ReadSecretFromConsole()
    {
        var sb = new StringBuilder();
        while (true)
        {
            var key = _console.ReadKey(true);

            if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                _console.WriteLine();
                return null;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                _console.WriteLine();
                break;
            }

            if (key.Key is ConsoleKey.Backspace or ConsoleKey.Delete)
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
