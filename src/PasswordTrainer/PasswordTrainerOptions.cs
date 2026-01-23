using System.ComponentModel.DataAnnotations;

namespace PasswordTrainer;

public class PasswordTrainerOptions
{
    public const string SectionName = "Trainer";

    [Required]
    public string DataPath { get; init; } = string.Empty;

    [Required]
    public string SecretsPath { get; init; } = string.Empty;

    [Range(1, 100)]
    public int RateLimitingPermitLimit { get; init; } = 15;

    [Range(1, 60)]
    public int RateLimitingWindowMinutes { get; init; } = 5;

    [RegularExpression(@"^\/[a-zA-Z0-9\-\/]*$")]
    public string? PathBase { get; init; }

    public string GetPepperFilePath() => Path.Combine(this.SecretsPath, "pepper_secret");

    public string GetPinHashFilePath() => Path.Combine(this.SecretsPath, "app_pin_hash");

    public string GetSecretsFilePath() => Path.Combine(this.DataPath, "secrets.json");
}