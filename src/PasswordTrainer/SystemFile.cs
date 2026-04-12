using System.Diagnostics.CodeAnalysis;

namespace PasswordTrainer;

[ExcludeFromCodeCoverage]
internal sealed class SystemFile : IFile
{
    public bool Exists(string? path) => File.Exists(path);

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) =>
        File.ReadAllBytesAsync(path, cancellationToken);

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) =>
        File.ReadAllTextAsync(path, cancellationToken);

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) =>
        File.WriteAllBytesAsync(path, bytes, cancellationToken);

    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default) =>
        File.WriteAllTextAsync(path, contents, cancellationToken);
}
