# Network Time Protocol (NTP) Command Line Interface (CLI)

.NET implementation of NTP via a command line interface.

- Inspiration: [Build your own NTP Client Code Challenge](https://codingchallenges.fyi/challenges/challenge-ntp/)
- [RFC 5905 - Network Time Protocol Version 4](https://datatracker.ietf.org/doc/html/rfc5905)

## Build

1. Ensure .NET 8.0 SDK is installed and available
2. Run `make ntpc` to output the Windows, Mac OS, and Linux binaries.

Depending upon your architecture use one of:
- `ntpc_win-x64.exe` for Windows
- `ntpc_osx-x64` for Mac OS
- `ntpc_linux-x64` for Linux

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
