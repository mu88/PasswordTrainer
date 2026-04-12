using System.Text;
using System.Text.Json;
using FluentAssertions;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PasswordTrainer;

namespace Tests.Unit;

[TestFixture]
[Category("Unit")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PasswordCheckServiceTests
{
    private const string ValidPin = "1234";
    private const string ValidId = "svc-test";

    // DataProtectionCommonExtensions.Unprotect(string) Base64Url-decodes before calling Unprotect(byte[]).
    // The file content must be a valid Base64Url string; the mock intercepts the byte[] overload.
    private const string EncryptedSecretsFileContent = "dGVzdA"; // base64url of "test", content doesn't matter

    private static readonly byte[] Pepper = new byte[32]; // all-zero pepper for test hashes
    private static readonly string ValidPasswordBase64 = Convert.ToBase64String("svcpw"u8.ToArray());
    private static string _pinHash = null!;
    private static string _passwordHash = null!;

    private IFile _file = null!;
    private IDataProtectionProvider _dp = null!;
    private IDataProtector _protector = null!;
    private IOptions<PasswordTrainerOptions> _options = null!;
    private PasswordCheckService _sut = null!;

    [OneTimeSetUp]
    public static void ComputeHashes()
    {
        var pinBytes = Encoding.UTF8.GetBytes(ValidPin);
        _pinHash = Argon2.Hash(pinBytes, Pepper);
        Array.Clear(pinBytes, 0, pinBytes.Length);

        var pwBytes = "svcpw"u8.ToArray();
        _passwordHash = Argon2.Hash(pwBytes, Pepper);
    }

    [SetUp]
    public void SetUp()
    {
        _file = Substitute.For<IFile>();
        _dp = Substitute.For<IDataProtectionProvider>();
        _protector = Substitute.For<IDataProtector>();
        _dp.CreateProtector("pw-store").Returns(_protector);

        var opts = new PasswordTrainerOptions { DataPath = "/data", SecretsPath = "/secrets" };
        _options = Substitute.For<IOptions<PasswordTrainerOptions>>();
        _options.Value.Returns(opts);

        var secretsJson = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [ValidId] = _passwordHash
        });

        // Use PasswordTrainerOptions helper methods so paths match what production code generates
        // (Path.Combine uses OS-specific directory separator on Windows).
        _file.ReadAllBytesAsync(opts.GetPepperFilePath(), Arg.Any<CancellationToken>()).Returns(Pepper);
        _file.ReadAllTextAsync(opts.GetPinHashFilePath(), Arg.Any<CancellationToken>()).Returns(_pinHash);
        _file.ReadAllTextAsync(opts.GetSecretsFilePath(), Arg.Any<CancellationToken>())
            .Returns(EncryptedSecretsFileContent);

        // Unprotect(string) extension method Base64Url-decodes then calls Unprotect(byte[]) — mock the byte[] overload.
        _protector.Unprotect(Arg.Any<byte[]>()).Returns(Encoding.UTF8.GetBytes(secretsJson));

        _sut = new PasswordCheckService(_file, _dp, _options, NullLogger<PasswordCheckService>.Instance);
    }

    [Test]
    public async Task CheckAsync_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, ValidId, ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task CheckAsync_WithWrongPin_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest("9999", ValidId, ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, "no-such-id", ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WithWrongPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, ValidId, Convert.ToBase64String("wrongpw"u8.ToArray()));

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WithInvalidBase64Password_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, ValidId, "not-valid-base64!!!");

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WhenDecryptionFails_ReturnsProblem()
    {
        // Arrange
        _protector.Unprotect(Arg.Any<byte[]>()).Throws(new InvalidOperationException("corrupted"));
        var request = new CheckRequest(ValidPin, ValidId, ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Test]
    public async Task CheckAsync_WithEmptyPin_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest(string.Empty, ValidId, ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WithTooShortPin_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest("12", ValidId, ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, string.Empty, ValidPasswordBase64);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, ValidId, string.Empty);

        // Act
        var result = await _sut.CheckAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task CheckAsync_ClearsPepperAfterUse_EvenOnEarlyReturn()
    {
        // Arrange – wrong PIN causes early return before VerifyPassword; pepper must still be cleared.
        // Use a non-zero pepper so the assertion is meaningful (all-zeros would pass even without clearing).
        var localPepper = new byte[32];
        localPepper[0] = 0xFF;
        var opts = new PasswordTrainerOptions { DataPath = "/data", SecretsPath = "/secrets" };
        _file.ReadAllBytesAsync(opts.GetPepperFilePath(), Arg.Any<CancellationToken>()).Returns(localPepper);

        var request = new CheckRequest("9999", ValidId, ValidPasswordBase64);

        // Act
        await _sut.CheckAsync(request, CancellationToken.None);

        // Assert – Array.Clear must zero out the pepper byte array even on early return
        localPepper.Should().AllBeEquivalentTo(0);
    }
}
