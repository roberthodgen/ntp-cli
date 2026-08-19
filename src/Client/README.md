# RobertHodgen.Ntp.Client

`RobertHodgen.Ntp.Client` is a .NET client library for querying Network Time Protocol servers and calculating clock offset and round-trip delay.

## Install

```bash
dotnet add package RobertHodgen.Ntp.Client
```

## Usage

```csharp
using RobertHodgen.Ntp.Client;

var client = new Client("pool.ntp.org");
var request = await client.ConnectAsync();

var offset = request.Theta();
var roundTripDelay = request.Delta();
```

## Features

- Query an NTP server over UDP.
- Parse NTP packet fields.
- Calculate theta, the client/server clock offset.
- Calculate delta, the round-trip delay.

## Repository

Source, issues, and release notes are available at https://github.com/roberthodgen/ntp-cli.
