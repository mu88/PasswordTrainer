namespace PasswordTrainer;

internal sealed record EncryptedPayload(string Nonce, string Ciphertext, string Tag);
