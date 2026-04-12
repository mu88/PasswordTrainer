using FluentAssertions;
using FluentAssertions.Web;

namespace Tests.Integration;

[TestFixture]
[Category("Integration")]
public class PathBaseTests
{
    [Test]
    public async Task HealthCheck_WithPathBaseConfigured_ShouldBeAccessibleViaPrefixedPath()
    {
        // Arrange
        await using var factory = new CheckEndpointWebApplicationFactory(pathBase: "/app");
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/app/healthz");

        // Assert
        response.Should().Be200Ok();
    }
}
