using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PasswordTrainer;

namespace Tests.Integration;

[TestFixture]
[Category("Integration")]
public class CheckEndpointDecryptionFailureTests
{
    private CheckEndpointWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        // secrets.json in corrupted-data contains a plain string, not a DataProtection-protected value.
        // PIN validation uses the same valid secrets dir so it passes, then decryption fails.
        _factory = new CheckEndpointWebApplicationFactory(
            dataPath: Path.Combine(AppContext.BaseDirectory, "testData", "corrupted-data"));
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Check_WhenSecretsFileCorrupted_ShouldReturn500()
    {
        // Arrange - secrets.json contains a plain string, not a DataProtection-protected value,
        // so Unprotect() throws CryptographicException and the endpoint returns a server error.
        var validPasswordBase64 = Convert.ToBase64String("helloworld"u8.ToArray());
        var request = new CheckRequest("1234", "systemtest", validPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
