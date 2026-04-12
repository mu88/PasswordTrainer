using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using PasswordTrainer;

namespace Tests.Unit;

[TestFixture]
[Category("Unit")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CheckRequestValidationTests
{
    [Test]
    public void Validate_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new CheckRequest("1234", "my-id", "mypassword");

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Test]
    public void Validate_WithEmptyPin_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest(string.Empty, "my-id", "mypassword");

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Pin)));
    }

    [Test]
    public void Validate_WithTooShortPin_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest("12", "my-id", "mypassword");

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Pin)));
    }

    [Test]
    public void Validate_WithTooLongPin_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest(new string('x', 129), "my-id", "mypassword");

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Pin)));
    }

    [Test]
    public void Validate_WithEmptyId_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest("1234", string.Empty, "mypassword");

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Id)));
    }

    [Test]
    public void Validate_WithTooLongId_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest("1234", new string('x', 129), "mypassword");

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Id)));
    }

    [Test]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest("1234", "my-id", string.Empty);

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Password)));
    }

    [Test]
    public void Validate_WithTooLongPassword_ShouldFail()
    {
        // Arrange
        var request = new CheckRequest("1234", "my-id", new string('x', 129));

        // Act
        var isValid = Validate(request, out var results);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(result => result.MemberNames.Contains(nameof(CheckRequest.Password)));
    }

    private static bool Validate(CheckRequest request, out ICollection<ValidationResult> results)
    {
        results = [];
        return Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
    }
}
