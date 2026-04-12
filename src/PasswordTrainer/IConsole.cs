namespace PasswordTrainer;

internal interface IConsole
{
    void Write(string? value);

    void WriteLine(string? value = null);

    string? ReadLine();

    ConsoleKeyInfo ReadKey(bool intercept);
}
