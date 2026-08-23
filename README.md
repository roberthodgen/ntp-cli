# Network Time Protocol (NTP) Command Line Interface (CLI)

.NET implementation of NTP via a command line interface.

- Inspiration: [Build your own NTP Client Code Challenge](https://codingchallenges.fyi/challenges/challenge-ntp/)
- [RFC 5905 - Network Time Protocol Version 4](https://datatracker.ietf.org/doc/html/rfc5905)
- Local RFC 5905 copy for standards reference: [docs/rfc5905.txt](docs/rfc5905.txt)

## Build

1. Ensure .NET 10 SDK is installed and available
2. Restore and build the solution:

```bash
dotnet restore ntp-cli.sln
dotnet build ntp-cli.sln
```

3. Run the tests:

```bash
dotnet test ntp-cli.sln
```

Build a release binary for your platform and architecture:

```bash
dotnet publish src/Cli -c Release --runtime osx-arm64 -o build/ntpc_macos-arm64
```

Replace `osx-arm64` with the runtime identifier for your platform and architecture:
- `win-x64` for Windows x64
- `linux-x64` for Linux x64
- `osx-x64` for macOS x64
- `osx-arm64` for macOS arm64

## NuGet Package

Only the `RobertHodgen.Ntp.Client` project is packaged for NuGet.

Build the package locally:

```bash
dotnet pack src/Client -c Release -o build /p:PackageVersion=0.1.0-local
```

The package and symbols package are written to `build/`.

See [docs/releasing.md](docs/releasing.md) for the release process.

## Usage

Check an NTP server for a time offset:
```
$ ntpc check
[12:01:53.7253150 INF] Network Time Protocol (NTP) Command Line Interface (CLI)
[12:01:53.8207750 INF] Using host: pool.ntp.org
[12:01:53.9681930 INF] Theta: 00:00:00.0156178
[12:01:53.9689850 INF] Delta: 00:00:00.0969010
```

Get help:
```
$ ntpc --help
Description:
  NTP CLI

Usage:
  ntpc [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  check  Check an NTP server for a time offset
```
