using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using PasswordTrainer;

namespace Tests.System;

[Category("System")]
public class SystemTests
{
    private const string SubPath = "/trainer";
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private DockerClient _dockerClient;
    private IContainer? _container;

    [SetUp]
    public void Setup()
    {
        _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        _cancellationToken = _cancellationTokenSource.Token;
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }

    [TearDown]
    public async Task Teardown()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
        {
            return; // no need to clean up on GitHub Actions runners
        }

        // If the test passed, clean up the container and image. Otherwise, keep them for investigation.
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed && _container is not null)
        {
            await _container.StopAsync(_cancellationToken);
            await _container.DisposeAsync();
            await _dockerClient.Images.DeleteImageAsync(_container.Image.FullName, new ImageDeleteParameters { Force = true }, _cancellationToken);
        }

        _dockerClient.Dispose();
        _cancellationTokenSource.Dispose();
    }

    [Test]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP014:Use a single instance of HttpClient", Justification = "Just a single test, not a perf issue")]
    public async Task AppRunningInDocker_ShouldBeHealthy()
    {
        // Arrange
        var containerImageTag = GenerateContainerImageTag();
        await BuildDockerImageOfAppAsync(containerImageTag, _cancellationToken);
        _container = await StartAppContainerAsync(containerImageTag, _cancellationToken);
        var httpClient = new HttpClient { BaseAddress = GetAppBaseAddress(_container) };

        // Act
        var healthCheckResponse = await httpClient.GetAsync("healthz", _cancellationToken);
        var appResponse = await httpClient.GetAsync("/", _cancellationToken);
        var passwordCheckResponse = await httpClient.PostAsJsonAsync("/check", new CheckRequest("1234", "systemtest", Convert.ToBase64String("helloworld"u8.ToArray())), _cancellationToken);
        var healthCheckToolResult = await _container.ExecAsync(["dotnet", "/app/mu88.HealthCheck.dll", $"http://127.0.0.1:8080{SubPath}/healthz"], _cancellationToken);

        // Assert
        await LogsShouldNotContainWarningsAsync(_container, _cancellationToken);
        await HealthCheckShouldBeHealthyAsync(healthCheckResponse, _cancellationToken);
        await AppShouldRunAsync(appResponse, _cancellationToken);
        passwordCheckResponse.Should().Be200Ok();
        healthCheckToolResult.ExitCode.Should().Be(0);
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
                        .WithEnvironment("Trainer__PathBase", SubPath)
                        .WithPortBinding(8080, true)
                        .WithBindMount(secretsPath, "/secrets", AccessMode.ReadOnly)
                        .WithBindMount(dataPath, "/data", AccessMode.ReadOnly)
                        .WithWaitStrategy(Wait.ForUnixContainer()
                                              .UntilMessageIsLogged("Content root path: /app", s => s.WithTimeout(TimeSpan.FromSeconds(30))))
                        .Build();

        await container.StartAsync(cancellationToken);
        return container;
    }

    private static async Task BuildDockerImageOfAppAsync(string containerImageTag,
                                                         CancellationToken cancellationToken)
    {
        var rootDirectory = GetRootPath();
        var projectFile = Path.Join(rootDirectory, "src", "PasswordTrainer", "PasswordTrainer.csproj");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments =
                    $"publish {projectFile} --os linux --arch amd64 " +
                    "/t:PublishContainersForMultipleFamilies " +
                    $"/p:ReleaseVersion={containerImageTag} " +
                    "/p:IsRelease=false " +
                    "/p:DoNotApplyGitHubScope=true",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        while (await process.StandardOutput.ReadLineAsync(cancellationToken) is { } line)
        {
            Console.WriteLine(line);
        }

        await process.WaitForExitAsync(cancellationToken);
        process.ExitCode.Should().Be(0);
    }

    private static Uri GetAppBaseAddress(IContainer container) => new($"http://{container.Hostname}:{container.GetMappedPublicPort(8080)}{SubPath}");

    private static async Task AppShouldRunAsync(HttpResponseMessage appResponse, CancellationToken cancellationToken)
    {
        appResponse.Should().Be200Ok();
        (await appResponse.Content.ReadAsStringAsync(cancellationToken))
            .Should()
            .Contain("<title>Password Trainer</title>");
    }

    private static async Task HealthCheckShouldBeHealthyAsync(HttpResponseMessage healthCheckResponse,
                                                              CancellationToken cancellationToken)
    {
        healthCheckResponse.Should().Be200Ok();
        (await healthCheckResponse.Content.ReadAsStringAsync(cancellationToken))
            .Should()
            .Be("Healthy");
    }

    private static async Task LogsShouldNotContainWarningsAsync(IContainer container,
                                                                CancellationToken cancellationToken)
    {
        var (stdout, stderr) = await container.GetLogsAsync(ct: cancellationToken);
        Console.WriteLine(stdout);
        Console.WriteLine(stderr);
        stdout.Should().NotContain("warn:");
    }

    private static string GetRootPath() => Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName ?? throw new NullReferenceException();

    [SuppressMessage("Design", "MA0076:Do not use implicit culture-sensitive ToString in interpolated strings", Justification = "Okay for me")]
    private static string GenerateContainerImageTag() => $"system-test-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
}