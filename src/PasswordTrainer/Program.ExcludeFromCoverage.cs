using System.Diagnostics.CodeAnalysis;

// Excludes the top-level statements in Program.cs from coverage analysis.
// Program.cs is a composition root only — all testable business logic lives in
// PasswordCheckService and SecretInitializationWorker, both of which are fully covered.
// The CLI bootstrap path is separately excluded via CliBootstrap's [ExcludeFromCodeCoverage].
[ExcludeFromCodeCoverage]
internal partial class Program // NOSONAR: top-level statements mandate global namespace (S3903) and non-static class (S1118)
{
}
