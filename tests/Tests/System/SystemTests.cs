using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using mu88.Shared.Testing.Docker;
using mu88.Shared.Testing.SystemTests;
using NUnit.Framework;
using PasswordTrainer;

namespace Tests.System;

[Category("System")]
public class SystemTests : SystemTestsBase
{
    protected override string SubPath => "/trainer";

    [Test]
    public async Task AppRunningInDocker_ShouldBeHealthy()
    {
        // Arrange
        var containerImageTag = DockerImageBuilder.GenerateContainerImageTag();
        await BuildDockerImageOfAppAsync(containerImageTag, CancellationToken);
        Container = await StartAppContainerAsync(containerImageTag, CancellationToken);

        // Act
        var passwordCheckResponse =
            await HttpClient.PostAsJsonAsync("/check", new CheckRequest("1234", "systemtest", Convert.ToBase64String("helloworld"u8.ToArray())), CancellationToken);

        // Assert
        await LogsShouldNotContainWarningsAsync(CancellationToken);
        await HealthCheckShouldSucceedAsync(CancellationToken);
        await AppShouldRunAsync(CancellationToken, "Password Trainer");
        passwordCheckResponse.Should().Be200Ok();
    }

    private static async Task<IContainer> StartAppContainerAsync(string imageTag, CancellationToken cancellationToken)
    {
        var rootDirectory = GetRootPath();
        var testDataPath = Path.Join(rootDirectory, "tests", "Tests", "testData");
        var secretsPath = Path.Join(testDataPath, "secrets");
        var dataPath = Path.Join(testDataPath, "data");

        var network = new NetworkBuilder().Build();
        await network.CreateAsync(cancellationToken);

        var container = new ContainerBuilder($"passwordtrainer:{imageTag}-chiseled")
            .WithNetwork(network)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("Trainer__DataPath", "/data")
            .WithEnvironment("Trainer__SecretsPath", "/secrets")
            .WithEnvironment("Trainer__PathBase", "/trainer")
            .WithPortBinding(8080, true)
            .WithBindMount(secretsPath, "/secrets", AccessMode.ReadOnly)
            .WithBindMount(dataPath, "/data", AccessMode.ReadOnly)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Content root path: /app", s => s.WithTimeout(TimeSpan.FromSeconds(30))))
            .Build();

        await container.StartAsync(cancellationToken);
        return container;
    }

    private static async Task BuildDockerImageOfAppAsync(string containerImageTag, CancellationToken cancellationToken)
    {
        var rootDirectory = GetRootPath();
        var projectFile = Path.Join(rootDirectory, "src", "PasswordTrainer", "PasswordTrainer.csproj");
        await DockerImageBuilder.BuildAsync(projectFile, containerImageTag, "passwordtrainer", rootDirectory, cancellationToken);
    }

    private static string GetRootPath() => Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName ?? throw new NullReferenceException();
}
