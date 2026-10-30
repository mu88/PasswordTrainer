using System.Diagnostics.CodeAnalysis;

// Excludes the top-level statements in Program.cs from coverage analysis.
// Program.cs is a composition root only — all testable business logic lives in
// PasswordCheckService and SecretInitializationWorker, both of which are fully covered.
// The CLI bootstrap path is separately excluded via CliBootstrap's [ExcludeFromCodeCoverage].
[ExcludeFromCodeCoverage]
#pragma warning disable SA1106 // StyleCopAnalyzers misidentifies the C# 12 semicolon-bodied type declaration below as an empty statement
internal partial class Program; // NOSONAR: top-level statements mandate global namespace (S3903) and non-static class (S1118)
#pragma warning restore SA1106
