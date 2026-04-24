using FluentAssertions;
using PasswordTrainer;

namespace Tests.Unit;

[TestFixture]
[Category("Unit")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PasswordTrainerOptionsTests
{
    [Test]
    public void GetPepperFilePath_ShouldCombineSecretsPathWithPepperSecretFilename()
    {
        // Arrange
        var options = new PasswordTrainerOptions { DataPath = "/data", SecretsPath = "/secrets" };

        // Act
        var result = options.GetPepperFilePath();

        // Assert
        result.Should().Be(Path.Combine("/secrets", "pepper_secret"));
    }

    [Test]
    public void GetPinHashFilePath_ShouldCombineSecretsPathWithAppPinHashFilename()
    {
        // Arrange
        var options = new PasswordTrainerOptions { DataPath = "/data", SecretsPath = "/secrets" };

        // Act
        var result = options.GetPinHashFilePath();

        // Assert
        result.Should().Be(Path.Combine("/secrets", "app_pin_hash"));
    }

    [Test]
    public void GetSecretsFilePath_ShouldCombineDataPathWithSecretsJsonFilename()
    {
        // Arrange
        var options = new PasswordTrainerOptions { DataPath = "/data", SecretsPath = "/secrets" };

        // Act
        var result = options.GetSecretsFilePath();

        // Assert
        result.Should().Be(Path.Combine("/data", "secrets.json"));
    }

    [Test]
    public void DataPath_DefaultValue_ShouldBeEmpty()
    {
        // Arrange / Act
        var options = new PasswordTrainerOptions();

        // Assert
        options.DataPath.Should().Be(string.Empty);
    }

    [Test]
    public void SecretsPath_DefaultValue_ShouldBeEmpty()
    {
        // Arrange / Act
        var options = new PasswordTrainerOptions();

        // Assert
        options.SecretsPath.Should().Be(string.Empty);
    }
}
