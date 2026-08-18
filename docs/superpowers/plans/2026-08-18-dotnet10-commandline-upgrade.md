# .NET 10 and System.CommandLine Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the CLI, client library, and tests to .NET 10, move the CLI to stable `System.CommandLine`, add macOS arm64 release output, and add initial NuGet package metadata for the client library.

**Architecture:** Keep the CLI as the executable project and the client as a separate library project. Apply target-framework and package metadata changes in project files, adapt only the CLI command wiring required by stable `System.CommandLine`, and update release packaging through the existing `Makefile`.

**Tech Stack:** .NET 10, C#, System.CommandLine 2.0.11, Serilog, xUnit, Shouldly, Makefile-based `dotnet publish` release artifacts.

**Spec:** `docs/superpowers/specs/2026-08-18-dotnet10-commandline-upgrade-design.md`

## Global Constraints

- Target `net10.0` for `src/Cli`, `src/Client`, and `src/Tests/Client`.
- Use `System.CommandLine` version `2.0.11`.
- Preserve the CLI `check` command, `--verbose` option, Ctrl+C cancellation behavior, and Serilog output.
- Do not multi-target the client library.
- Do not add compatibility shims for older runtimes.
- Do not publish a NuGet package.
- Do not add CI/CD publishing, NuGet API keys, or trusted publishing setup.
- Release artifacts must be `ntpc_win-x64.exe`, `ntpc_linux-x64`, `ntpc_macos-x64`, and `ntpc_macos-arm64`.
- Client package metadata must use `PackageId` `RobertHodgen.Ntp.Client` and repository URL `https://github.com/roberthodgen/ntp-cli`.
- Use the existing root `README.md` as the package readme.
- Do not commit unless the user explicitly requests a commit; use `git diff` checkpoints instead.

---

## File Structure

- Modify `src/Cli/RobertHodgen.Ntp.Cli.csproj`: app target framework, runtime identifiers, and stable `System.CommandLine` package version.
- Modify `src/Client/RobertHodgen.Ntp.Client.csproj`: library target framework and NuGet package metadata.
- Modify `src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj`: test target framework.
- Modify `src/Cli/Program.cs`: stable `System.CommandLine` API usage while preserving runtime behavior.
- Modify `Makefile`: .NET 10 publish output paths, four runtime targets, renamed macOS artifacts, and cleanup.
- Modify `README.md`: .NET 10 build prerequisite and release artifact names so the package readme is accurate.
- Modify `.gitignore`: ignore renamed macOS release artifacts.

---

### Task 1: Retarget Projects and Add Client Package Metadata

**Files:**
- Modify: `src/Cli/RobertHodgen.Ntp.Cli.csproj`
- Modify: `src/Client/RobertHodgen.Ntp.Client.csproj`
- Modify: `src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj`

**Interfaces:**
- Consumes: Existing project references between CLI, Client, and Tests.
- Produces: All projects target `net10.0`; CLI references `System.CommandLine` `2.0.11`; Client carries NuGet metadata and packs root `README.md`.

- [ ] **Step 1: Update the CLI project file**

Replace the property and package sections in `src/Cli/RobertHodgen.Ntp.Cli.csproj` so the complete file is:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <DebugType>embedded</DebugType>
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64;osx-arm64</RuntimeIdentifiers>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Client\RobertHodgen.Ntp.Client.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="System.CommandLine" Version="2.0.11" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Update the client project file**

Replace `src/Client/RobertHodgen.Ntp.Client.csproj` so the complete file is:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PackageId>RobertHodgen.Ntp.Client</PackageId>
    <Title>RobertHodgen.Ntp.Client</Title>
    <Authors>Robert Hodgen</Authors>
    <Description>Network Time Protocol client library for querying server time and calculating clock offset and round-trip delay.</Description>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <RepositoryUrl>https://github.com/roberthodgen/ntp-cli</RepositoryUrl>
    <PackageProjectUrl>https://github.com/roberthodgen/ntp-cli</PackageProjectUrl>
    <PackageTags>ntp;time;network;client</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.0.2" />
  </ItemGroup>

  <ItemGroup>
    <None Include="..\..\README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Update the test project file**

In `src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj`, change:

```xml
<TargetFramework>net8.0</TargetFramework>
```

to:

```xml
<TargetFramework>net10.0</TargetFramework>
```

- [ ] **Step 4: Restore packages**

Run: `dotnet restore ntp-cli.sln`

Expected: restore succeeds and resolves `System.CommandLine` `2.0.11`.

- [ ] **Step 5: Build to expose CLI API incompatibility**

Run: `dotnet build ntp-cli.sln`

Expected: the build may fail in `src/Cli/Program.cs` because beta4 APIs such as `AddOption`, `SetHandler`, or direct `InvokeAsync(args)` are no longer valid against stable `System.CommandLine`.

- [ ] **Step 6: Inspect the project diff checkpoint**

Run: `git diff -- src/Cli/RobertHodgen.Ntp.Cli.csproj src/Client/RobertHodgen.Ntp.Client.csproj src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj`

Expected: diff shows only target framework, runtime identifier, package version, and client metadata changes.

---

### Task 2: Adapt CLI Command Wiring to Stable System.CommandLine

**Files:**
- Modify: `src/Cli/Program.cs`

**Interfaces:**
- Consumes: `System.CommandLine` `2.0.11` from Task 1 and `RobertHodgen.Ntp.Client.Client.ConnectAsync(CancellationToken)`.
- Produces: A CLI that parses `check`, reads `--verbose`, invokes the client with cancellation, and returns the stable parser invocation exit code.

- [ ] **Step 1: Replace beta API calls with stable API calls**

Replace the contents of `src/Cli/Program.cs` with:

```csharp
using System.CommandLine;
using RobertHodgen.Ntp.Client;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var levelSwitch = new LoggingLevelSwitch
{
    MinimumLevel = LogEventLevel.Information,
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fffffff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        standardErrorFromLevel: LogEventLevel.Warning)
    .CreateLogger();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Log.Warning("Stopping...");
    cts.Cancel();
    e.Cancel = true;
};

Log.Information("Network Time Protocol (NTP) Command Line Interface (CLI)");

var rootCommand = new RootCommand("NTP CLI");
var verboseOption = new Option<bool>("--verbose")
{
    Description = "Enable verbose logging"
};
var checkCommand = new Command("check", "Check an NTP server for a time offset");

rootCommand.Add(checkCommand);
checkCommand.Add(verboseOption);

checkCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var verbose = parseResult.GetValue(verboseOption);
    if (verbose)
    {
        levelSwitch.MinimumLevel = LogEventLevel.Verbose;
    }

    var request = await new Client().ConnectAsync(cancellationToken);

    Log.Debug("Server response headers:");
    request.ServerResponse.Header.LogDebugData();

    Log.Debug("Local receive timestamp: {receiveTimestamp:O}", request.ServerResponse.DestinationTimestamp);

    Log.Information($"Theta: {request.Theta():c} (absolute time difference between client and server clocks)");
    Log.Information($"Delta: {request.Delta():c} (round-trip delay)");
});

return await rootCommand.Parse(args).InvokeAsync(cancellationToken: cts.Token);
```

- [ ] **Step 2: Build the solution**

Run: `dotnet build ntp-cli.sln`

Expected: build succeeds with `using System.CommandLine;` as the only `System.CommandLine` using directive.

- [ ] **Step 3: Verify root help still works**

Run: `dotnet run --project src/Cli/RobertHodgen.Ntp.Cli.csproj -- --help`

Expected: output includes `Description:`, `NTP CLI`, and `check`.

- [ ] **Step 4: Verify check help still exposes verbose**

Run: `dotnet run --project src/Cli/RobertHodgen.Ntp.Cli.csproj -- check --help`

Expected: output includes `Check an NTP server for a time offset` and `--verbose`.

- [ ] **Step 5: Inspect the Program.cs diff checkpoint**

Run: `git diff -- src/Cli/Program.cs`

Expected: diff shows only `System.CommandLine` API adaptation and removal of the old inline comment on verbose logging.

---

### Task 3: Update Release Publishing Matrix

**Files:**
- Modify: `Makefile`
- Modify: `.gitignore`

**Interfaces:**
- Consumes: CLI target framework `net10.0` and runtime identifiers `win-x64;linux-x64;osx-x64;osx-arm64` from Task 1.
- Produces: `make ntpc` builds four ignored root artifacts named `ntpc_win-x64.exe`, `ntpc_linux-x64`, `ntpc_macos-x64`, and `ntpc_macos-arm64`.

- [ ] **Step 1: Replace the Makefile contents**

Replace `Makefile` with:

```make
.PHONY: ntpc ntpc_win-x64.exe ntpc_linux-x64 ntpc_macos-x64 ntpc_macos-arm64
ntpc: ntpc_win-x64.exe ntpc_linux-x64 ntpc_macos-x64 ntpc_macos-arm64

ntpc_win-x64.exe:
	dotnet publish --runtime win-x64
	cp src/Cli/bin/Release/net10.0/win-x64/publish/RobertHodgen.Ntp.Cli.exe ntpc_win-x64.exe

ntpc_linux-x64:
	dotnet publish --runtime linux-x64
	cp src/Cli/bin/Release/net10.0/linux-x64/publish/RobertHodgen.Ntp.Cli ntpc_linux-x64

ntpc_macos-x64:
	dotnet publish --runtime osx-x64
	cp src/Cli/bin/Release/net10.0/osx-x64/publish/RobertHodgen.Ntp.Cli ntpc_macos-x64

ntpc_macos-arm64:
	dotnet publish --runtime osx-arm64
	cp src/Cli/bin/Release/net10.0/osx-arm64/publish/RobertHodgen.Ntp.Cli ntpc_macos-arm64

.PHONY: clean
clean:
	dotnet clean -c Release
	rm -f ntpc_win-x64.exe ntpc_linux-x64 ntpc_osx-x64 ntpc_macos-x64 ntpc_macos-arm64
```

- [ ] **Step 2: Run the release build**

Run: `make ntpc`

Expected: the command publishes four runtimes and copies four root artifacts.

- [ ] **Step 3: Verify release artifacts exist**

Run: `test -f ntpc_win-x64.exe && test -f ntpc_linux-x64 && test -f ntpc_macos-x64 && test -f ntpc_macos-arm64`

Expected: command exits with status `0`.

- [ ] **Step 4: Inspect the Makefile diff checkpoint**

Run: `git diff -- Makefile`

Expected: diff shows `net10.0` paths, macOS artifact rename, arm64 target addition, and cleanup update.

- [ ] **Step 5: Update ignored release artifact names**

In `.gitignore`, keep the old macOS artifact line and add the new macOS artifact lines:

```gitignore
ntpc_osx-x64
ntpc_macos-x64
ntpc_macos-arm64
```

- [ ] **Step 6: Verify new macOS artifacts are ignored**

Run: `git check-ignore -v ntpc_macos-x64 ntpc_macos-arm64`

Expected: both paths are reported as ignored by `.gitignore`.

---

### Task 4: Update README for .NET 10 and Artifact Names

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes: release artifact names from Task 3.
- Produces: A root README that is accurate for repository users and valid as the client package readme.

- [ ] **Step 1: Update the build prerequisite and artifact list**

In `README.md`, replace this section:

```markdown
1. Ensure .NET 8.0 SDK is installed and available
2. Run `make ntpc` to output the Windows, Mac OS, and Linux binaries.

Depending upon your architecture use one of:
- `ntpc_win-x64.exe` for Windows
- `ntpc_osx-x64` for Mac OS
- `ntpc_linux-x64` for Linux
```

with:

```markdown
1. Ensure .NET 10 SDK is installed and available
2. Run `make ntpc` to output the Windows, macOS, and Linux binaries.

Depending upon your platform and architecture use one of:
- `ntpc_win-x64.exe` for Windows x64
- `ntpc_linux-x64` for Linux x64
- `ntpc_macos-x64` for macOS x64
- `ntpc_macos-arm64` for macOS arm64
```

- [ ] **Step 2: Verify README package inclusion by packing the client**

Run: `dotnet pack src/Client/RobertHodgen.Ntp.Client.csproj`

Expected: pack succeeds and produces a `.nupkg` under `src/Client/bin/Release` or `src/Client/bin/Debug`, depending on SDK defaults.

- [ ] **Step 3: Inspect the README diff checkpoint**

Run: `git diff -- README.md`

Expected: diff shows only .NET 10 prerequisite and artifact name updates.

---

### Task 5: Full Verification and Cleanup Check

**Files:**
- No source edits expected.
- Generated artifacts: root `ntpc_*` binaries, `bin/`, and `obj/` outputs are build artifacts.

**Interfaces:**
- Consumes: completed project, CLI, Makefile, and README changes from Tasks 1 through 4.
- Produces: verified solution upgrade with passing build, tests, publish, and pack checks.

- [ ] **Step 1: Restore the full solution**

Run: `dotnet restore ntp-cli.sln`

Expected: restore succeeds.

- [ ] **Step 2: Build the full solution**

Run: `dotnet build ntp-cli.sln`

Expected: build succeeds with zero errors.

- [ ] **Step 3: Run all tests**

Run: `dotnet test ntp-cli.sln`

Expected: all xUnit tests pass.

- [ ] **Step 4: Pack the client locally**

Run: `dotnet pack src/Client/RobertHodgen.Ntp.Client.csproj`

Expected: pack succeeds without publishing anything.

- [ ] **Step 5: Build all release artifacts**

Run: `make ntpc`

Expected: all four root artifacts are created: `ntpc_win-x64.exe`, `ntpc_linux-x64`, `ntpc_macos-x64`, and `ntpc_macos-arm64`.

- [ ] **Step 6: Verify CLI help after publish work**

Run: `dotnet run --project src/Cli/RobertHodgen.Ntp.Cli.csproj -- --help`

Expected: output includes `NTP CLI` and `check`.

- [ ] **Step 7: Verify no unintended tracked files changed**

Run: `git diff --stat`

Expected: tracked changes are limited to `.gitignore`, `README.md`, `Makefile`, three project files, `src/Cli/Program.cs`, the design spec, and this implementation plan.

- [ ] **Step 8: Verify build artifacts are ignored or untracked only**

Run: `git status --short`

Expected: root `ntpc_*` binaries do not appear as tracked modifications. New plan/spec docs and source/config changes appear as normal tracked or untracked worktree changes.

- [ ] **Step 9: Report completion evidence**

Record the exact commands that passed in the final response:

```text
dotnet restore ntp-cli.sln
dotnet build ntp-cli.sln
dotnet test ntp-cli.sln
dotnet pack src/Client/RobertHodgen.Ntp.Client.csproj
make ntpc
dotnet run --project src/Cli/RobertHodgen.Ntp.Cli.csproj -- --help
```
