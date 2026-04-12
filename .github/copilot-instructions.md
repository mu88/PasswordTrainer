# PasswordTrainer — Repo Context

## Architecture
- Single binary that serves both as CLI (secret initialization via `CliBootstrap`) and HTTP API (check endpoint). Both entry points share `Program.cs` top-level statements.
- `PasswordCheckService` is the central service for all password-check logic. Do not introduce another service class or a `ProgramCore`-style workaround.
- `IFile`/`SystemFile` and `IConsole`/`SystemConsole` humble objects are already established — inject them everywhere, never call `File.*` or `Console.*` directly. Do **not** add `System.IO.Abstractions` or other third-party I/O abstraction libraries.

## Integration Tests — WebApplicationFactory
- `CheckEndpointWebApplicationFactory` is the only WAF class. It **must** override `HostingEnvironment.ApplicationName` to `typeof(Program).Assembly.GetName().Name!` inside `ConfigureWebHost` — without this, `MapStaticAssets()` cannot find the static-assets manifest in the test output directory and the WAF fails to start.

## Test Organization
- Tests live in `tests/Tests/` with subdirectories: `Unit/`, `Integration/`, `System/`. Always place new test files in the correct subdirectory.

## Security Invariants — Never Change These
- Passwords are Argon2-hashed with a file-based pepper. Pepper path comes from `PasswordTrainerOptions.GetPepperFilePath()`.
- Secrets are encrypted with `DataProtectionProvider.CreateProtector("pw-store")`. The string `"pw-store"` is part of the persisted data contract — changing it makes existing encrypted files permanently unreadable.
- `CheckRequest.Password` carries **Base64-encoded raw bytes**, not plain text. Any test password must be encoded: `Convert.ToBase64String(Encoding.UTF8.GetBytes("mypassword"))` — or use `"mypassword"u8.ToArray()` and then Base64-encode.
