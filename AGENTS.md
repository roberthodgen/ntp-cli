# AGENTS.md

## Project Shape
- .NET 10 solution: `ntp-cli.sln` contains the CLI app, NTP client library, and client tests.
- CLI entrypoint is `src/Cli/Program.cs`; it wires `System.CommandLine`, Serilog, Ctrl+C cancellation, and the `check` command.
- Library code lives in `src/Client`; NTP packet/header/field encoding and parsing is under `src/Client/Remote`.
- Tests live under `src/Tests/Client`, use xUnit with Shouldly, and currently reference only the client project.
- The NuGet package version is the `version` field in root `package.json`; the release workflow reads it there and requires the release tag to be `v<version>`.

## Standards Reference
- `docs/rfc5905.txt` is a local copy of RFC 5905, the NTPv4 protocol and algorithms specification. Read it when answering questions or making changes related to NTP standards, packet fields, timestamp formats, protocol modes, or wire behavior.

## RFC Compliance
All NTP client code must be compliant with RFC 5905. Key sections to reference when implementing or modifying NTP-related code:
- **Section 6 (Data Types)** — 64-bit fixed-point timestamps, 32-bit fixed-point timestamps, timedelta, and millisecond types. Verify timestamp field implementations against the NTP timestamp format (32-bit seconds + 32-bit fraction since 1900).
- **Section 7 (Data Structures)** — Packet header layout (li, vn, mode, stratum, poll, precision, delay, dispersion, refid, and timestamp fields). When adding or modifying packet encoding/parsing, cross-check field positions, sizes, and endianness against Section 7.3.
- **Section 8 (On-Wire Protocol)** — Client/server exchange rules, mode values, leap second indicator handling, version number negotiation, and Kiss-o'-Death packets. Client code must handle all defined server mode responses and leap indicator values per spec.
- **Section 3 (Protocol Modes)** — Client mode (mode 3) behavior, including request/response exchange and dynamic server discovery.

When writing or reviewing code in `src/Client/Remote`, verify the implementation against the RFC before considering the work complete. Test cases should cover RFC-defined edge cases such as leap second indicators, version negotiation, and Kiss-o'-Death codes.

## Commands
- Restore/build the full solution with `dotnet restore ntp-cli.sln` then `dotnet build ntp-cli.sln`.
- Run all tests with `dotnet test ntp-cli.sln`.
- Run a focused xUnit test with `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~LeapIndicatorTests`.
- Build a release CLI binary with `dotnet publish src/Cli -c Release --runtime <rid> -o build/<name>`; runtime IDs are `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`. The release workflow uses `-p:PublishAot=true` for macOS.
- Build the NuGet client package with `dotnet pack src/Client -c Release -o build /p:PackageVersion=<semver>`.
- Remove local build outputs with `dotnet clean -c Release` and `rm -rf build`.

## NTP Timestamp Strategy
The client request sends `OriginTimestamp.Zero` (all zeros) in the origin field.
This is intentional: the server echoes back the origin timestamp in its response,
and `Theta()`/`Delta()` read `t0` from `ServerResponse.Header.OriginTimestamp.Value`
— the server's copy, not the client's original packet. The goal is to get as close
to actual wire timestamps as possible. The client captures `t3` (destination) at
receive time in `Client.ReadResponse()`, and `t1`/`t2` come from the server's
response. `t0` comes from the server-echoed origin. If you modify this flow,
verify that `t0`/`t1`/`t2`/`t3` all represent the closest possible approximation
to the actual wire crossing times per RFC 5905 Section 8.

## Repo-Specific Gotchas
- `build/` and legacy root `ntpc_*` binaries are build artifacts listed in `.gitignore`; do not edit or commit generated binaries.
- The CLI project has `PublishSingleFile`, `SelfContained`, and runtime IDs set in `src/Cli/RobertHodgen.Ntp.Cli.csproj`; prefer `dotnet publish --runtime <rid>` when checking packaged behavior.
- NTP numeric fields encode in network byte order; existing field types usually reverse `BitConverter` output on little-endian machines.
- There is no CI, formatter config, lockfile, or repo-local OpenCode/Copilot/Cursor instruction file at the root; rely on the solution, project files, and `.github/workflows/release.yml` as sources of truth.
