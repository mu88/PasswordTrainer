using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Web;
using PasswordTrainer;

namespace Tests.Integration;

[TestFixture]
[Category("Integration")]
public class RateLimitingTests
{
    private const string ValidPin = "1234";
    private const string ValidId = "systemtest";
    private static readonly string ValidPasswordBase64 = Convert.ToBase64String("helloworld"u8.ToArray());

    [Test]
    public async Task Check_WhenRateLimitExceeded_ShouldReturn429()
    {
        // Arrange
        await using var factory = new CheckEndpointWebApplicationFactory(rateLimitingPermitLimit: 1);
        using var client = factory.CreateClient();
        var request = new CheckRequest(ValidPin, ValidId, ValidPasswordBase64);

        // Act
        var firstResponse = await client.PostAsJsonAsync("/check", request);
        var secondResponse = await client.PostAsJsonAsync("/check", request);

        // Assert
        firstResponse.Should().Be200Ok();
        secondResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
