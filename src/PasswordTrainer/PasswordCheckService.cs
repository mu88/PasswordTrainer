using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace PasswordTrainer;

internal sealed partial class PasswordCheckService
{
    private const string InvalidCredentials = "Invalid credentials";

    private readonly IFile _file;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IOptions<PasswordTrainerOptions> _options;
    private readonly ILogger<PasswordCheckService> _logger;

    public PasswordCheckService(
        IFile file,
        IDataProtectionProvider dataProtectionProvider,
        IOptions<PasswordTrainerOptions> options,
        ILogger<PasswordCheckService> logger)
    {
        _file = file;
        _dataProtectionProvider = dataProtectionProvider;
        _options = options;
        _logger = logger;
    }

    internal async Task<IResult> CheckAsync(CheckRequest request, CancellationToken cancellationToken)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            return Results.BadRequest(validationResults.Select(result => result.ErrorMessage));
        }

        // Pepper is sensitive key material: always cleared in finally, even on early return.
        var pepper = await _file.ReadAllBytesAsync(_options.Value.GetPepperFilePath(), cancellationToken);
        try
        {
            var expectedPinHash = await _file.ReadAllTextAsync(_options.Value.GetPinHashFilePath(), cancellationToken);
            if (!VerifyPin(request.Pin, expectedPinHash, pepper))
            {
                return Results.BadRequest(InvalidCredentials);
            }

            string decrypted;
            try
            {
                var protector = _dataProtectionProvider.CreateProtector("pw-store");
                decrypted = protector.Unprotect(
                    await _file.ReadAllTextAsync(_options.Value.GetSecretsFilePath(), cancellationToken));
            }
            catch (Exception ex)
            {
                // Stryker disable once Statement : verifying log calls requires a real logger sink; the decryption error is tested via the HTTP 500 response
                LogDecryptionFailure(_logger, ex);
                return Results.Problem("Server error");
            }

            var store = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted)
                        ?? new Dictionary<string, string>(StringComparer.Ordinal);
            if (!store.TryGetValue(request.Id, out var expectedPasswordHash))
            {
                return Results.BadRequest(InvalidCredentials);
            }

            return VerifyPassword(request.Password, expectedPasswordHash, pepper)
                ? Results.Ok()
                : Results.BadRequest(InvalidCredentials);
        }
        finally
        {
            Array.Clear(pepper, 0, pepper.Length);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Decryption of secrets failed", SkipEnabledCheck = true)]
    private static partial void LogDecryptionFailure(ILogger logger, Exception exception);

    private static bool VerifyPin(string pin, string expectedHash, byte[] pepper)
    {
        var pinBytes = Encoding.UTF8.GetBytes(pin);
        try
        {
            return Argon2.Verify(expectedHash, pinBytes, pepper);
        }

        // Stryker disable once Block : removing the finally block skips Array.Clear; this security hygiene is not observable from outside the method
        finally
        {
            // Stryker disable once Statement : clearing local byte arrays is security hygiene; not observable from outside the method
            Array.Clear(pinBytes, 0, pinBytes.Length);
        }
    }

    private static bool VerifyPassword(string passwordBase64, string expectedHash, byte[] pepper)
    {
        byte[] passwordBytes;
        try
        {
            passwordBytes = Convert.FromBase64String(passwordBase64);
        }
        catch (FormatException)
        {
            return false;
        }

        try
        {
            return Argon2.Verify(expectedHash, passwordBytes, pepper);
        }

        // Stryker disable once Block : removing the finally block skips Array.Clear; this security hygiene is not observable from outside the method
        finally
        {
            // Stryker disable once Statement : clearing local byte arrays is security hygiene; not observable from outside the method
            Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }
}
