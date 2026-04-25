using System.Security.Cryptography;
using System.Text.Json;
using FluentAssertions;
using PasswordTrainer;

namespace Tests.Unit;

[TestFixture]
[Category("Unit")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AesGcmSecretsEncryptionTests
{
    private static readonly byte[] TestPepper = new byte[32];
    private readonly AesGcmSecretsEncryption _sut = new();

    [Test]
    public void Encrypt_ThenDecrypt_ReturnsOriginalPlaintext()
    {
        // Arrange
        const string plaintext = """{"svc-test":"$argon2id$v=19$m=65536,t=3,p=4$test"}""";

        // Act
        var ciphertext = _sut.Encrypt(TestPepper, plaintext);
        var result = _sut.Decrypt(TestPepper, ciphertext);

        // Assert
        result.Should().Be(plaintext);
    }

    [Test]
    public void Encrypt_ProducesValidJsonPayload()
    {
        // Arrange
        const string plaintext = "hello";

        // Act
        var ciphertext = _sut.Encrypt(TestPepper, plaintext);

        // Assert
        var payload = JsonSerializer.Deserialize<EncryptedPayload>(ciphertext);
        payload.Should().NotBeNull();
        payload!.Nonce.Should().NotBeNullOrEmpty();
        payload.Ciphertext.Should().NotBeNullOrEmpty();
        payload.Tag.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(payload.Nonce).Should().HaveCount(AesGcm.NonceByteSizes.MaxSize);
        Convert.FromBase64String(payload.Tag).Should().HaveCount(AesGcm.TagByteSizes.MaxSize);
    }

    [Test]
    public void Encrypt_WithSameInput_ProducesDifferentCiphertextEachTime()
    {
        // Arrange
        const string plaintext = "same input";

        // Act
        var ciphertext1 = _sut.Encrypt(TestPepper, plaintext);
        var ciphertext2 = _sut.Encrypt(TestPepper, plaintext);

        // Assert — random nonce ensures uniqueness
        ciphertext1.Should().NotBe(ciphertext2);
    }

    [Test]
    public void Decrypt_WithDifferentPepper_ThrowsCryptographicException()
    {
        // Arrange
        var otherPepper = new byte[32];
        otherPepper[0] = 0xFF;
        var ciphertext = _sut.Encrypt(TestPepper, "secret");

        // Act
        var act = () => _sut.Decrypt(otherPepper, ciphertext);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Test]
    public void Decrypt_WithTamperedCiphertext_ThrowsCryptographicException()
    {
        // Arrange
        var ciphertext = _sut.Encrypt(TestPepper, "secret");
        var payload = JsonSerializer.Deserialize<EncryptedPayload>(ciphertext)!;
        var tampered = payload with { Ciphertext = Convert.ToBase64String(new byte[Convert.FromBase64String(payload.Ciphertext).Length]) };
        var tamperedJson = JsonSerializer.Serialize(tampered);

        // Act
        var act = () => _sut.Decrypt(TestPepper, tamperedJson);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Test]
    public void Decrypt_WithTamperedTag_ThrowsCryptographicException()
    {
        // Arrange
        var ciphertext = _sut.Encrypt(TestPepper, "secret");
        var payload = JsonSerializer.Deserialize<EncryptedPayload>(ciphertext)!;
        var tamperedTag = Convert.FromBase64String(payload.Tag);
        tamperedTag[0] ^= 0x01; // flip one bit — guaranteed to differ regardless of original value
        var tampered = payload with { Tag = Convert.ToBase64String(tamperedTag) };
        var tamperedJson = JsonSerializer.Serialize(tampered);

        // Act
        var act = () => _sut.Decrypt(TestPepper, tamperedJson);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Test]
    public void Decrypt_WithInvalidJson_ThrowsCryptographicException()
    {
        // Arrange
        const string notJson = "not-a-valid-aes-gcm-payload";

        // Act
        var act = () => _sut.Decrypt(TestPepper, notJson);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Test]
    public void Decrypt_WithNullPayload_ThrowsCryptographicException()
    {
        // Arrange — valid JSON that deserializes to null
        const string nullJson = "null";

        // Act
        var act = () => _sut.Decrypt(TestPepper, nullJson);

        // Assert
        act.Should().Throw<CryptographicException>().WithMessage("Invalid encrypted payload");
    }
}
