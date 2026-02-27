using System.ComponentModel.DataAnnotations;

namespace PasswordTrainer;

internal record CheckRequest(
    [property: Required]
    [property: MinLength(4)]
    [property: MaxLength(128)]
    string Pin,
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(128)]
    string Id,
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(128)]
    string Password);
