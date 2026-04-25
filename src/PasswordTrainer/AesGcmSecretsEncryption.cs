using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordTrainer;

internal sealed class AesGcmSecretsEncryption : ISecretsEncryption
{
    private static readonly byte[] HkdfInfo = "pw-store"u8.ToArray();
    private static readonly byte[] HkdfSalt = "password-trainer-v1"u8.ToArray();
    private static readonly byte[] AadContext = "PasswordTrainer-secrets-v1"u8.ToArray();

    public string Encrypt(byte[] pepper, string plaintext)
    {
        var key = DeriveKey(pepper);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        try
        {
            var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];

            using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, AadContext);

            return JsonSerializer.Serialize(new EncryptedPayload(
                Convert.ToBase64String(nonce),
                Convert.ToBase64String(ciphertext),
                Convert.ToBase64String(tag)));
        }
        finally
        {
            // Stryker disable once Statement : clearing derived key is security hygiene; not observable from outside the method
            Array.Clear(key);

            // Stryker disable once Statement : clearing plaintext bytes is security hygiene; not observable from outside the method
            Array.Clear(plaintextBytes);
        }
    }

    public string Decrypt(byte[] pepper, string ciphertext)
    {
        byte[] key = Array.Empty<byte>();
        byte[] plaintextBytes = Array.Empty<byte>();
        try
        {
            var payload = JsonSerializer.Deserialize<EncryptedPayload>(ciphertext)
                          ?? throw new CryptographicException("Invalid encrypted payload");

            key = DeriveKey(pepper);
            var nonce = Convert.FromBase64String(payload.Nonce);
            var encryptedBytes = Convert.FromBase64String(payload.Ciphertext);
            var tag = Convert.FromBase64String(payload.Tag);
            plaintextBytes = new byte[encryptedBytes.Length];

            using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
            aes.Decrypt(nonce, encryptedBytes, tag, plaintextBytes, AadContext);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
        catch (CryptographicException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Failed to decrypt payload", ex);
        }
        finally
        {
            // Stryker disable once Statement : clearing derived key is security hygiene; not observable from outside the method
            Array.Clear(key);

            // Stryker disable once Statement : clearing decrypted bytes is security hygiene; not observable from outside the method
            Array.Clear(plaintextBytes);
        }
    }

    private static byte[] DeriveKey(byte[] pepper) =>
        HKDF.DeriveKey(HashAlgorithmName.SHA256, pepper, outputLength: 32, salt: HkdfSalt, info: HkdfInfo);
}
