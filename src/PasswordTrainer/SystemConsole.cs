using System.Diagnostics.CodeAnalysis;

namespace PasswordTrainer;

[ExcludeFromCodeCoverage]
internal sealed class SystemConsole : IConsole
{
    public void Write(string? value) => Console.Write(value);

    public void WriteLine(string? value = null) => Console.WriteLine(value);

    public string? ReadLine() => Console.ReadLine();

    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
}
