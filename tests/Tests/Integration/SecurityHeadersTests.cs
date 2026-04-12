using FluentAssertions;

namespace Tests.Integration;

[TestFixture]
[Category("Integration")]
public class SecurityHeadersTests
{
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
    public async Task HealthEndpoint_ShouldReturnSecurityHeaders()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("DENY");
        response.Headers.Should().ContainKey("X-Content-Type-Options")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("nosniff");
        response.Headers.Should().ContainKey("Referrer-Policy")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("no-referrer");
        response.Headers.Should().ContainKey("Content-Security-Policy")
            .WhoseValue.Should().ContainSingle().Which.Should().Be("default-src 'self'; script-src 'self'; style-src 'self'");
    }
}
