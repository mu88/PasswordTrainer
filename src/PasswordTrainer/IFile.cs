namespace PasswordTrainer;

internal interface IFile
{
    bool Exists(string? path);

    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);
}
