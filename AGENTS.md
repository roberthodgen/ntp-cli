# AGENTS.md

## Project Shape
- .NET 8 solution: `ntp-cli.sln` contains the CLI app, NTP client library, and client tests.
- CLI entrypoint is `src/Cli/Program.cs`; it wires `System.CommandLine`, Serilog, Ctrl+C cancellation, and the `check` command.
- Library code lives in `src/Client`; NTP packet/header/field encoding and parsing is under `src/Client/Remote`.
- Tests live under `src/Tests/Client`, use xUnit with Shouldly, and currently reference only the client project.

## Commands
- Restore/build the full solution with `dotnet restore ntp-cli.sln` then `dotnet build ntp-cli.sln`.
- Run all tests with `dotnet test ntp-cli.sln`.
- Run a focused xUnit test with `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~LeapIndicatorTests`.
- Build release binaries for Windows, macOS, and Linux with `make ntpc`; this publishes self-contained single-file outputs under `build/` as `ntpc_win-x64.exe`, `ntpc_linux-x64`, `ntpc_macos-x64`, and `ntpc_macos-arm64`.
- Remove release outputs with `make clean`.

## Repo-Specific Gotchas
- `build/` and legacy root `ntpc_*` binaries are build artifacts listed in `.gitignore`; do not edit or commit generated binaries.
- The CLI project has `PublishSingleFile`, `SelfContained`, and runtime IDs set in `src/Cli/RobertHodgen.Ntp.Cli.csproj`; prefer `dotnet publish --runtime <rid>` or `make ntpc` when checking packaged behavior.
- NTP numeric fields encode in network byte order; existing field types usually reverse `BitConverter` output on little-endian machines.
- There is no CI, formatter config, lockfile, or repo-local OpenCode/Copilot/Cursor instruction file at the root; rely on the solution, project files, and Makefile as sources of truth.
