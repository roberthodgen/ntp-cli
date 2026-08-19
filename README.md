# Network Time Protocol (NTP) Command Line Interface (CLI)

.NET implementation of NTP via a command line interface.

- Inspiration: [Build your own NTP Client Code Challenge](https://codingchallenges.fyi/challenges/challenge-ntp/)
- [RFC 5905 - Network Time Protocol Version 4](https://datatracker.ietf.org/doc/html/rfc5905)

## Build

1. Ensure .NET 10 SDK is installed and available
2. Run `make ntpc` to output the Windows, macOS, and Linux binaries under `build/`.

Depending upon your platform and architecture use one of:
- `build/ntpc_win-x64.exe` for Windows x64
- `build/ntpc_linux-x64` for Linux x64
- `build/ntpc_macos-x64` for macOS x64
- `build/ntpc_macos-arm64` for macOS arm64

## NuGet Package

Only the `RobertHodgen.Ntp.Client` project is packaged for NuGet.

Build the package locally:

```bash
make package VERSION=0.1.0-local
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
