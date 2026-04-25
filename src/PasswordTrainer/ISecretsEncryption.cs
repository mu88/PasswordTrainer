namespace PasswordTrainer;

internal interface ISecretsEncryption
{
    string Encrypt(byte[] pepper, string plaintext);

    string Decrypt(byte[] pepper, string ciphertext);
}
