# .NET 10 and System.CommandLine Upgrade Design

## Summary

Upgrade the solution to .NET 10, move the CLI from the beta `System.CommandLine` package to the latest stable package, add a macOS arm64 release artifact, and begin preparing the client project to become a standalone NuGet package.

The client project will target `net10.0`, not `netstandard2.0`. The priority is modern runtime performance and a clean path toward library packaging, rather than maximum compatibility with older project types.

## Goals

- Target `net10.0` for the CLI, client library, and client tests.
- Upgrade `System.CommandLine` from `2.0.0-beta4.22272.1` to latest stable `2.0.11`.
- Preserve the current CLI behavior for `check`, `--verbose`, Ctrl+C cancellation, and Serilog output.
- Add a macOS arm64 release build.
- Rename macOS release artifacts from `osx` naming to `macos` naming.
- Keep generated release artifacts ignored by git.
- Add initial NuGet package metadata to the client project so it is closer to publishable as a standalone library.
- Keep client implementation modern and performance-oriented by preserving .NET 10 APIs where appropriate.

## Non-Goals

- Do not publish a NuGet package in this change.
- Do not add CI/CD publishing, NuGet API keys, or trusted publishing setup yet.
- Do not multi-target the client library.
- Do not redesign the public client API beyond changes required for the upgrade.
- Do not introduce broad compatibility shims for older runtimes.

## Current State

The solution contains three projects:

- `src/Cli/RobertHodgen.Ntp.Cli.csproj` targets `net8.0` and references `System.CommandLine` `2.0.0-beta4.22272.1`.
- `src/Client/RobertHodgen.Ntp.Client.csproj` targets `net8.0` and references `Serilog`.
- `src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj` targets `net8.0` and references the client project.

The CLI release `Makefile` currently publishes three self-contained single-file artifacts:

- `ntpc_win-x64.exe`
- `ntpc_osx-x64`
- `ntpc_linux-x64`

The CLI project currently declares these runtime identifiers:

- `win-x64`
- `osx-x64`
- `linux-x64`

## Target Project Shape

All projects should target `net10.0`:

- CLI: `net10.0`
- Client: `net10.0`
- Client tests: `net10.0`

The CLI remains the executable application and continues to reference the client project. The client remains a separate class library project and should not take on CLI-specific dependencies or responsibilities.

## System.CommandLine Upgrade

The CLI should move from the beta package to stable `System.CommandLine` `2.0.11`.

The current command surface must be preserved:

- Root command description: `NTP CLI`
- `check` command: checks an NTP server for time offset
- `--verbose` option: enables verbose logging
- Ctrl+C cancellation: requests cancellation and prevents immediate process termination
- Logging behavior: Serilog console output remains the user-facing output path

If the stable package has API differences from beta4, adapt `Program.cs` with the smallest compatible change. Avoid broad CLI restructuring unless the stable API requires it.

## Runtime Publishing

The CLI project should declare four runtime identifiers:

- `win-x64`
- `linux-x64`
- `osx-x64`
- `osx-arm64`

The release output artifacts should be:

- `ntpc_win-x64.exe`
- `ntpc_linux-x64`
- `ntpc_macos-x64`
- `ntpc_macos-arm64`

The `Makefile` should publish using `net10.0` output paths and copy the four release artifacts to the repository root. The `clean` target should remove all four current artifacts and the legacy `ntpc_osx-x64` artifact name. `.gitignore` should ignore the renamed macOS artifacts and the legacy macOS artifact name so generated binaries do not appear as untracked source changes.

## Client Library Package Readiness

The client project should begin carrying package metadata so it is closer to NuGet-ready. This change should add metadata only; actual package publication is out of scope.

Initial metadata should include:

- `PackageId`: `RobertHodgen.Ntp.Client`
- `Title`: `RobertHodgen.Ntp.Client`
- `Authors`: `Robert Hodgen`
- `Description`: concise description of the library as an NTP client for querying server time and calculating offset/delay
- `PackageReadmeFile`: `README.md`, using the existing repository README as package readme input
- `RepositoryUrl`: `https://github.com/roberthodgen/ntp-cli`
- `PackageProjectUrl`: `https://github.com/roberthodgen/ntp-cli`
- `PackageTags`: tags such as `ntp`, `time`, `network`, `client`
- XML documentation generation: optional for this change; enable only if the build stays clean without adding broad documentation work

Implementation verified the repository remote as `git@github.com:roberthodgen/ntp-cli.git`; use the corresponding HTTPS URL in package metadata.

Package metadata should not imply the package is stable or published. If version metadata is added, keep it conservative and explicit.

## Client Performance Posture

The client should stay on modern .NET APIs where they are already appropriate:

- Keep cancellation-aware DNS and socket APIs if they compile under `net10.0`.
- Keep `Memory<byte>` and range-based parsing unless tests or profiling reveal a concrete problem.
- Keep modern C# syntax supported by the .NET 10 SDK.

The upgrade should not replace these APIs with older compatibility paths.

## Testing and Verification

Verification commands:

```bash
dotnet restore ntp-cli.sln
dotnet build ntp-cli.sln
dotnet test ntp-cli.sln
make ntpc
```

The implementation is successful when:

- Restore succeeds.
- Build succeeds for the full solution.
- Tests pass.
- `make ntpc` produces all four release artifacts.
- The CLI still accepts the `check` command and `--verbose` option.
- The client project can be packed locally with the new package metadata.

Package-readiness verification should include a local pack check:

```bash
dotnet pack src/Client/RobertHodgen.Ntp.Client.csproj
```

This pack check should not publish anything.

## Risks and Mitigations

- `System.CommandLine` stable API may differ from beta4. Mitigate by making the smallest `Program.cs` adaptation needed to preserve behavior.
- .NET 10 SDK availability is required. The local environment has .NET SDK `10.0.300` installed.
- XML documentation generation may expose missing-doc warnings if warnings are elevated later. In this implementation, enable XML documentation only if the current build remains clean; otherwise defer it to a later package-hardening change.

## Implementation Boundaries

The implementation should be small and mechanical:

- Edit project files for target frameworks, dependencies, runtime identifiers, and package metadata.
- Edit `Program.cs` only for `System.CommandLine` API compatibility.
- Edit `Makefile` for runtime matrix, output paths, renamed macOS artifacts, and cleanup.
- Edit `.gitignore` for renamed macOS artifacts.
- Run the verification commands.

Any larger API redesign, NuGet publishing workflow, README/package documentation expansion, or CI setup should be handled as a follow-up design and implementation plan.
