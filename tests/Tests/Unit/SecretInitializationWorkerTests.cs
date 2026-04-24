using System.Text;
using FluentAssertions;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using PasswordTrainer;

namespace Tests.Unit;

[TestFixture]
[Category("Unit")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SecretInitializationWorkerTests
{
    private const string DataPath = "/data";
    private const string SecretsPath = "/secrets";

    private IConsole _console = null!;
    private IFile _file = null!;
    private IDataProtectionProvider _dataProtectionProvider = null!;
    private IDataProtector _dataProtector = null!;
    private IHostApplicationLifetime _lifetime = null!;
    private SecretInitializationWorker _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _console = Substitute.For<IConsole>();
        _file = Substitute.For<IFile>();
        _dataProtectionProvider = Substitute.For<IDataProtectionProvider>();
        _dataProtector = Substitute.For<IDataProtector>();
        _lifetime = Substitute.For<IHostApplicationLifetime>();

        _dataProtectionProvider.CreateProtector("pw-store").Returns(_dataProtector);
        _dataProtector.Protect(Arg.Any<byte[]>()).Returns(Array.Empty<byte>());

        var options = Substitute.For<IOptions<PasswordTrainerOptions>>();
        options.Value.Returns(new PasswordTrainerOptions { DataPath = DataPath, SecretsPath = SecretsPath });

        _sut = new SecretInitializationWorker(options, _lifetime, _console, _file, _dataProtectionProvider);
    }

    [TearDown]
    public void TearDown() => _sut?.Dispose();

    [Test]
    public async Task ExecuteAsync_WithValidInputAndNoPepperFile_ShouldGenerateAndWritePepperAndCallStopApplication()
    {
        // Arrange
        _file.Exists(Path.Combine(SecretsPath, "pepper_secret")).Returns(false);
        SetupConsoleForPin("1234");
        _console.ReadLine().Returns("0"); // zero passwords

        // Act
        await _sut.StartAsync(CancellationToken.None);
        await _sut.ExecuteTask!;

        // Assert
        await _file.Received(1).WriteAllBytesAsync(
            Path.Combine(SecretsPath, "pepper_secret"),
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());
        await _file.Received(1).WriteAllTextAsync(
            Path.Combine(SecretsPath, "app_pin_hash"),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _file.Received(1).WriteAllTextAsync(
            Path.Combine(DataPath, "secrets.json"),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        _lifetime.Received(1).StopApplication();
        _console.Received(1).WriteLine("=== PasswordTrainer Init Mode ===");
        _console.Received(1).Write("Enter new App-PIN: ");
        _console.Received(1).Write("How many passwords? ");
        _console.Received(1).WriteLine("=== Init Complete ===");
        // Verify the newline written after masked PIN input (ReadSecretFromConsole on Enter)
        _console.Received(1).WriteLine();
    }

    [Test]
    public async Task ExecuteAsync_WithExistingPepperFile_ShouldReadPepperAndNotWriteIt()
    {
        // Arrange
        _file.Exists(Path.Combine(SecretsPath, "pepper_secret")).Returns(true);
        _file.ReadAllBytesAsync(Path.Combine(SecretsPath, "pepper_secret"), Arg.Any<CancellationToken>())
            .Returns(new byte[32]);
        SetupConsoleForPin("1234");
        _console.ReadLine().Returns("0");

        // Act
        await _sut.StartAsync(CancellationToken.None);
        await _sut.ExecuteTask!;

        // Assert
        await _file.DidNotReceive().WriteAllBytesAsync(
            Path.Combine(SecretsPath, "pepper_secret"),
            Arg.Any<byte[]>(),
            Arg.Any<CancellationToken>());
        _lifetime.Received(1).StopApplication();
    }

    [Test]
    public async Task ExecuteAsync_WithOnePassword_ShouldWriteEncryptedPasswordDict()
    {
        // Arrange
        _file.Exists(Arg.Any<string>()).Returns(false);
        _console.ReadKey(true).Returns(
            KeyInfo('1'),
            KeyInfo('2'),
            KeyInfo('3'),
            KeyInfo('4'),
            Enter(),     // PIN: 1234
            KeyInfo('p'),
            KeyInfo('a'),
            KeyInfo('s'),
            KeyInfo('s'),
            Enter());    // password: pass
        _console.ReadLine().Returns("1", "my-id"); // count=1, id=my-id

        // Act
        await _sut.StartAsync(CancellationToken.None);
        await _sut.ExecuteTask!;

        // Assert
        _dataProtector.Received(1).Protect(
            Arg.Is<byte[]>(bytes => Encoding.UTF8.GetString(bytes).Contains("my-id")));
        _lifetime.Received(1).StopApplication();
        _console.Received(1).Write("ID #1: ");
        _console.Received(1).Write("Password for 'my-id': ");
    }

    [Test]
    public async Task ExecuteAsync_WithEmptyPin_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _file.Exists(Arg.Any<string>()).Returns(false);
        _console.ReadKey(true).Returns(Enter()); // empty PIN

        // Act
        await _sut.StartAsync(CancellationToken.None);
        var act = async () => await _sut.ExecuteTask!;

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("App PIN must not be empty");
    }

    [Test]
    public async Task ExecuteAsync_WithNonNumericPasswordCount_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _file.Exists(Arg.Any<string>()).Returns(false);
        SetupConsoleForPin("1234");
        _console.ReadLine().Returns("abc"); // non-numeric count

        // Act
        await _sut.StartAsync(CancellationToken.None);
        var act = async () => await _sut.ExecuteTask!;

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Please enter a valid number.");
    }

    [Test]
    public async Task ExecuteAsync_WithEmptyPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _file.Exists(Arg.Any<string>()).Returns(false);
        _console.ReadKey(true).Returns(
            KeyInfo('1'),
            KeyInfo('2'),
            KeyInfo('3'),
            KeyInfo('4'),
            Enter(),  // PIN: 1234
            Enter()); // empty password
        _console.ReadLine().Returns("1", "my-id");

        // Act
        await _sut.StartAsync(CancellationToken.None);
        var act = async () => await _sut.ExecuteTask!;

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Password must not be empty");
    }

    [Test]
    public async Task ExecuteAsync_ReadSecretFromConsole_BackspaceRemovesLastChar()
    {
        // Arrange
        _file.Exists(Arg.Any<string>()).Returns(false);
        _console.ReadKey(true).Returns(
            KeyInfo('1'),
            KeyInfo('2'),
            Backspace(), // erase '2' → buffer: "1"
            KeyInfo('3'),
            KeyInfo('4'),
            KeyInfo('5'),
            Enter(),     // PIN: 1345
            KeyInfo('p'),
            Enter());
        _console.ReadLine().Returns("1", "my-id");

        // Capture pepper and hash so we can verify backspace actually removed the char
        var capturedPepper = Array.Empty<byte>();
        _file.When(f => f.WriteAllBytesAsync(
                Path.Combine(SecretsPath, "pepper_secret"), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturedPepper = ci.ArgAt<byte[]>(1));
        var capturedPinHash = string.Empty;
        _file.When(f => f.WriteAllTextAsync(
                Path.Combine(SecretsPath, "app_pin_hash"), Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturedPinHash = ci.ArgAt<string>(1));

        // Act
        await _sut.StartAsync(CancellationToken.None);
        await _sut.ExecuteTask!;

        // Assert — no exception thrown means PIN "1345" was accepted
        _lifetime.Received(1).StopApplication();
        // Verify that the hash was computed for "1345" (not "12345" or "12\0345")
        capturedPepper.Should().NotBeEmpty();
        capturedPinHash.Should().NotBeNullOrEmpty();
        Argon2.Verify(capturedPinHash, Encoding.UTF8.GetBytes("1345"), capturedPepper)
            .Should().BeTrue("the hash must match PIN '1345' — backspace must decrement sb.Length");
        // Verify that the backspace visual feedback was written
        _console.Received(1).Write("\b \b");
        // Verify that exactly 5 chars were echoed as '*' for PIN (1,2,3,4,5) plus 1 for password 'p'
        _console.Received(6).Write("*");
    }

    [Test]
    public async Task ExecuteAsync_ReadSecretFromConsole_BackspaceOnEmptyInputIsIgnored()
    {
        // Arrange
        _file.Exists(Arg.Any<string>()).Returns(false);
        _console.ReadKey(true).Returns(
            Backspace(), // ignored (buffer empty)
            KeyInfo('1'),
            KeyInfo('2'),
            KeyInfo('3'),
            KeyInfo('4'),
            Enter(),     // PIN: 1234
            KeyInfo('p'),
            Enter());
        _console.ReadLine().Returns("1", "my-id");

        // Act
        await _sut.StartAsync(CancellationToken.None);
        await _sut.ExecuteTask!;

        // Assert
        _lifetime.Received(1).StopApplication();
        // Verify no backspace feedback was written (the Backspace on empty buffer is silently ignored)
        _console.DidNotReceive().Write("\b \b");
        // 4 '*' for PIN + 1 '*' for password 'p'
        _console.Received(5).Write("*");
    }

    [Test]
    public async Task ExecuteAsync_WithNullIdFromConsole_ShouldThrowInvalidOperationException()
    {
        // Arrange — ReadLine returns null for the ID slot, triggering the null-guard
        _file.Exists(Arg.Any<string>()).Returns(false);
        SetupConsoleForPin("1234");
        _console.ReadLine().Returns("1", (string?)null); // count=1, then null for ID

        // Act
        await _sut.StartAsync(CancellationToken.None);
        var act = async () => await _sut.ExecuteTask!;

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ID must not be empty");
    }

    private static ConsoleKeyInfo KeyInfo(char c) =>
        new(c, ConsoleKey.A, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Enter() =>
        new('\0', ConsoleKey.Enter, shift: false, alt: false, control: false);

    private static ConsoleKeyInfo Backspace() =>
        new('\0', ConsoleKey.Backspace, shift: false, alt: false, control: false);

    private void SetupConsoleForPin(string pin)
    {
        var keys = pin.Select(KeyInfo).Append(Enter()).ToArray();
        _console.ReadKey(true).Returns(keys[0], keys[1..]);
    }
}