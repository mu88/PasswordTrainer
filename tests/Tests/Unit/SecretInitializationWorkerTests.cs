using System.Text;
using FluentAssertions;
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

        // Act
        await _sut.StartAsync(CancellationToken.None);
        await _sut.ExecuteTask!;

        // Assert — no exception thrown means PIN "1345" was accepted
        _lifetime.Received(1).StopApplication();
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