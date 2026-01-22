using System.ComponentModel.DataAnnotations;

namespace PasswordTrainer;

internal record CheckRequest(
    [property: Required, MinLength(4), MaxLength(128)] string Pin,
    [property: Required, MinLength(1), MaxLength(128)] string Id,
    [property: Required, MinLength(1), MaxLength(128)] string Password
);