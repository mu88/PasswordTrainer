using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Web;
using PasswordTrainer;

namespace Tests.Integration;

[TestFixture]
[Category("Integration")]
public class CheckEndpointTests
{
    // Test credentials matching testData/secrets (PIN: 1234, id: systemtest, password: helloworld)
    private const string ValidPin = "1234";
    private const string ValidId = "systemtest";
    private static readonly string ValidPasswordBase64 = Convert.ToBase64String("helloworld"u8.ToArray());

    private CheckEndpointWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CheckEndpointWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Check_WithValidCredentials_ShouldReturn200()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, ValidId, ValidPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be200Ok();
    }

    [Test]
    public async Task Check_WithWrongPin_ShouldReturn400()
    {
        // Arrange
        var request = new CheckRequest("9999", ValidId, ValidPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithWrongPassword_ShouldReturn400()
    {
        // Arrange
        var wrongPasswordBase64 = Convert.ToBase64String("wrongpassword"u8.ToArray());
        var request = new CheckRequest(ValidPin, ValidId, wrongPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithNonExistentId_ShouldReturn400()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, "doesnotexist", ValidPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithInvalidBase64Password_ShouldReturn400()
    {
        // Arrange - regression test for FormatException bug (would previously return HTTP 500)
        var request = new CheckRequest(ValidPin, ValidId, "not-valid-base64!!!");

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithEmptyPin_ShouldReturn400()
    {
        // Arrange
        var request = new CheckRequest(string.Empty, ValidId, ValidPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithTooShortPin_ShouldReturn400()
    {
        // Arrange
        var request = new CheckRequest("12", ValidId, ValidPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithEmptyId_ShouldReturn400()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, string.Empty, ValidPasswordBase64);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }

    [Test]
    public async Task Check_WithEmptyPassword_ShouldReturn400()
    {
        // Arrange
        var request = new CheckRequest(ValidPin, ValidId, string.Empty);

        // Act
        var response = await _client.PostAsJsonAsync("/check", request);

        // Assert
        response.Should().Be400BadRequest();
    }
}
