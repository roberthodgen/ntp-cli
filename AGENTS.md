# AGENTS.md

## Project Shape
- .NET 10 solution: `ntp-cli.sln` contains the CLI app, NTP client library, and client tests.
- CLI entrypoint is `src/Cli/Program.cs`; it wires `System.CommandLine`, Serilog, Ctrl+C cancellation, and the `check` command.
- Library code lives in `src/Client`; NTP packet/header/field encoding and parsing is under `src/Client/Remote`.
- Tests live under `src/Tests/Client`, use xUnit with Shouldly, and currently reference only the client project.
- The NuGet package version is the `version` field in root `package.json`; the release workflow reads it there and requires the release tag to be `v<version>`.

## Standards Reference
- `docs/rfc5905.txt` is a local copy of RFC 5905, the NTPv4 protocol and algorithms specification. Read it when answering questions or making changes related to NTP standards, packet fields, timestamp formats, protocol modes, or wire behavior.

## Commands
- Restore/build the full solution with `dotnet restore ntp-cli.sln` then `dotnet build ntp-cli.sln`.
- Run all tests with `dotnet test ntp-cli.sln`.
- Run a focused xUnit test with `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~LeapIndicatorTests`.
- Build a release CLI binary with `dotnet publish src/Cli -c Release --runtime <rid> -o build/<name>`; runtime IDs are `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`. The release workflow uses `-p:PublishAot=true` for macOS.
- Build the NuGet client package with `dotnet pack src/Client -c Release -o build /p:PackageVersion=<semver>`.
- Remove local build outputs with `dotnet clean -c Release` and `rm -rf build`.

## Repo-Specific Gotchas
- `build/` and legacy root `ntpc_*` binaries are build artifacts listed in `.gitignore`; do not edit or commit generated binaries.
- The CLI project has `PublishSingleFile`, `SelfContained`, and runtime IDs set in `src/Cli/RobertHodgen.Ntp.Cli.csproj`; prefer `dotnet publish --runtime <rid>` when checking packaged behavior.
- NTP numeric fields encode in network byte order; existing field types usually reverse `BitConverter` output on little-endian machines.
- There is no CI, formatter config, lockfile, or repo-local OpenCode/Copilot/Cursor instruction file at the root; rely on the solution, project files, and `.github/workflows/release.yml` as sources of truth.
