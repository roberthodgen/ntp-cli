# MonotonicClock Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace `DateTime.UtcNow`-based timestamp capture with a `MonotonicClock` that seeds from a single UTC reading then derives all subsequent timestamps from `Stopwatch.Elapsed` for hardware-level precision.

**Architecture:** Interface `IMonotonicClock` with `DateTime UtcNow` and `NtpTimestamp Capture()`. Production impl `MonotonicClock` captures `DateTime.UtcNow` once, starts `Stopwatch`, and computes timestamps from elapsed ticks. `NtpTimestamp.Now` is removed; all timestamp capture flows through the clock. `Client` takes `IMonotonicClock` as a constructor dependency.

**Tech Stack:** .NET 10, `System.Diagnostics.Stopwatch`, xUnit, Shouldly

**Spec:** Brainstormed design in conversation — interface-based monotonic clock replacing static `NtpTimestamp.Now`.

## Global Constraints

- NTP numeric fields encode in network byte order (big endian); field types reverse `BitConverter` output on little-endian machines.
- The NTP client request sends `OriginTimestamp.Zero` (all zeros) — server echoes back t0. Do not change this strategy.
- Test framework: xUnit with Shouldly. Tests reference the client project.
- Build: `dotnet build ntp-cli.sln`. Tests: `dotnet test ntp-cli.sln`.
- No new NuGet dependencies.

---

### Task 1: IMonotonicClock interface and MonotonicClock implementation

**Files:**
- Create: `src/Client/Remote/IMonotonicClock.cs`
- Create: `src/Client/Remote/MonotonicClock.cs`
- Create: `src/Tests/Client/Remote/MonotonicClockTests.cs`

**Interfaces:**
- Consumes: `NtpTimestamp` (existing)
- Produces: `IMonotonicClock` interface with `DateTime UtcNow` and `NtpTimestamp Capture()`

- [ ] **Step 1: Write the failing tests**

```csharp
// src/Tests/Client/Remote/MonotonicClockTests.cs
namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;

public class MonotonicClockTests
{
    [Fact]
    public void UtcNow_ReturnsTimeCloseToSystemUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var clock = new MonotonicClock();
        var after = DateTime.UtcNow.AddSeconds(1);

        var utcNow = clock.UtcNow;

        utcNow.ShouldBeGreaterThanOrEqualTo(before);
        utcNow.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public void Capture_ReturnsNonZeroTimestamp()
    {
        var clock = new MonotonicClock();
        var ts = clock.Capture();

        ts.Seconds.ShouldNotBe(0u);
    }

    [Fact]
    public void Capture_MultipleCalls_AreMonotonicallyIncreasing()
    {
        var clock = new MonotonicClock();
        var first = clock.Capture();
        System.Threading.Thread.Sleep(1);
        var second = clock.Capture();

        var combined = (long)second.Seconds << 32 | second.Fraction;
        var firstCombined = (long)first.Seconds << 32 | first.Fraction;
        combined.ShouldBeGreaterThan(firstCombined);
    }

    [Fact]
    public void Capture_RoundTripsWithinOneMillisecond()
    {
        var clock = new MonotonicClock();
        var ts = clock.Capture();

        var diff = (ts.ToDateTime() - clock.UtcNow).Duration();
        diff.ShouldBeLessThanOrEqualTo(TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void UtcNow_AfterDelay_ReflectsElapsedTicks()
    {
        var clock = new MonotonicClock();
        var initial = clock.UtcNow;
        System.Threading.Thread.Sleep(10);
        var later = clock.UtcNow;

        (later - initial).TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(8);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~MonotonicClockTests"`
Expected: Build failure — types don't exist yet.

- [ ] **Step 3: Write the interface**

```csharp
// src/Client/Remote/IMonotonicClock.cs
namespace RobertHodgen.Ntp.Client.Remote;

public interface IMonotonicClock
{
    DateTime UtcNow { get; }
    NtpTimestamp Capture();
}
```

Note: `NtpTimestamp` is in `RobertHodgen.Ntp.Client.Remote.Fields` — the interface lives in `RobertHodgen.Ntp.Client.Remote` so we need to either add a `using` or move the return type. Since `NtpTimestamp` is in a sub-namespace, the interface file needs `using RobertHodgen.Ntp.Client.Remote.Fields;`.

- [ ] **Step 4: Write the implementation**

```csharp
// src/Client/Remote/MonotonicClock.cs
namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

public sealed class MonotonicClock : IMonotonicClock
{
    static readonly uint NtpFractionDivisor = 4294967296u;
    static readonly double TicksPerNtpFraction = (double)Stopwatch.Frequency / NtpFractionDivisor;

    readonly DateTime _referenceTime;
    readonly uint _referenceSeconds;
    readonly uint _referenceFraction;
    readonly Stopwatch _stopwatch;

    public MonotonicClock()
    {
        _referenceTime = DateTime.UtcNow;
        var ntp = NtpTimestamp.FromDateTime(_referenceTime);
        _referenceSeconds = ntp.Seconds;
        _referenceFraction = ntp.Fraction;
        _stopwatch = Stopwatch.StartNew();
    }

    public DateTime UtcNow => _referenceTime + _stopwatch.Elapsed;

    public NtpTimestamp Capture()
    {
        var elapsedTicks = _stopwatch.ElapsedTicks;
        var elapsedSeconds = (uint)(elapsedTicks / (long)Stopwatch.Frequency);
        var remainingTicks = elapsedTicks % (long)Stopwatch.Frequency;
        var elapsedFraction = (uint)(remainingTicks * TicksPerNtpFraction);

        var totalFraction = (ulong)_referenceFraction + elapsedFraction;
        var carrySeconds = totalFraction / NtpFractionDivisor;
        var finalFraction = (uint)(totalFraction % NtpFractionDivisor);

        var finalSeconds = _referenceSeconds + elapsedSeconds + (uint)carrySeconds;

        return NtpTimestamp.Create(finalSeconds, finalFraction);
    }
}
```

This requires adding a factory method to `NtpTimestamp` — see Task 2.

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~MonotonicClockTests"`
Expected: All pass.

- [ ] **Step 6: Commit**

```bash
git add src/Client/Remote/IMonotonicClock.cs src/Client/Remote/MonotonicClock.cs src/Tests/Client/Remote/MonotonicClockTests.cs
git commit -m "feat: add IMonotonicClock interface and Stopwatch-based MonotonicClock"
```

### Task 2: Add NtpTimestamp.Create factory and remove NtpTimestamp.Now

**Files:**
- Modify: `src/Client/Remote/Fields/NtpTimestamp.cs`
- Modify: `src/Tests/Client/Remote/Fields/NtpTimestampTests.cs`

**Interfaces:**
- Consumes: Nothing new
- Produces: `NtpTimestamp.Create(uint seconds, uint fraction)` public factory; `NtpTimestamp.FromClock(IMonotonicClock)` convenience

- [ ] **Step 1: Write failing test for FromClock**

Add to `NtpTimestampTests.cs`:
```csharp
[Fact]
public void FromClock_ReturnsTimestampFromClock()
{
    var clock = new MonotonicClock();
    var before = DateTime.UtcNow.AddSeconds(-1);
    var ts = NtpTimestamp.FromClock(clock);
    var after = DateTime.UtcNow.AddSeconds(1);

    ts.ToDateTime().ShouldBeGreaterThanOrEqualTo(before);
    ts.ToDateTime().ShouldBeLessThanOrEqualTo(after);
}
```

- [ ] **Step 2: Remove the Now test**

Remove from `NtpTimestampTests.cs`:
```csharp
[Fact]
public void Now_ProducesTimestampCloseToCurrentUtcTime()
{
    var before = DateTime.UtcNow.AddSeconds(-1);
    var timestamp = NtpTimestamp.Now;
    var after = DateTime.UtcNow.AddSeconds(1);

    var dateTime = timestamp.ToDateTime();
    dateTime.ShouldBeGreaterThanOrEqualTo(before);
    dateTime.ShouldBeLessThanOrEqualTo(after);
}
```

- [ ] **Step 3: Modify NtpTimestamp**

In `NtpTimestamp.cs`:
- Remove `public static NtpTimestamp Now => FromDateTime(DateTime.UtcNow);`
- Add `public static NtpTimestamp Create(uint seconds, uint fraction) => new(seconds, fraction);`
- Add `public static NtpTimestamp FromClock(IMonotonicClock clock) => clock.Capture();`
- Add `using RobertHodgen.Ntp.Client.Remote;` at top (for `IMonotonicClock`)

- [ ] **Step 4: Run tests**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~NtpTimestampTests"`
Expected: All pass.

- [ ] **Step 5: Commit**

```bash
git add src/Client/Remote/Fields/NtpTimestamp.cs src/Tests/Client/Remote/Fields/NtpTimestampTests.cs
git commit -m "feat: add NtpTimestamp.Create and FromClock, remove Now"
```

### Task 3: Update TransmitTimestamp and OriginTimestamp to use IMonotonicClock

**Files:**
- Modify: `src/Client/Remote/Fields/TransmitTimestamp.cs`
- Modify: `src/Client/Remote/Fields/OriginTimestamp.cs`
- Modify: `src/Tests/Client/Remote/Fields/OriginTimestampTests.cs`
- Modify: `src/Tests/Client/Remote/Fields/TransmitTimestampTests.cs`

**Interfaces:**
- Consumes: `IMonotonicClock`, updated `NtpTimestamp`
- Produces: `TransmitTimestamp.FromClock(IMonotonicClock)`, `OriginTimestamp.SerializableFromClock(IMonotonicClock)`

- [ ] **Step 1: Update TransmitTimestamp**

In `TransmitTimestamp.cs`:
- Replace `public static TransmitTimestamp Now => new (NtpTimestamp.Now);` with `public static TransmitTimestamp FromClock(IMonotonicClock clock) => new (NtpTimestamp.FromClock(clock));`
- Add `using RobertHodgen.Ntp.Client.Remote;` at top

- [ ] **Step 2: Update OriginTimestamp**

In `OriginTimestamp.cs`:
- Replace `public static OriginTimestamp Now => new (NtpTimestamp.Now);` with `public static OriginTimestamp FromClock(IMonotonicClock clock) => new (NtpTimestamp.FromClock(clock));`
- Replace `public static OriginTimestamp SerializableNow => new (NtpTimestamp.Zero, () => NtpTimestamp.Now);` with `public static OriginTimestamp SerializableFromClock(IMonotonicClock clock) => new (NtpTimestamp.Zero, () => clock.Capture());`
- Update `CreateSerializableForTesting` signature: `internal static OriginTimestamp CreateSerializableForTesting(Func<NtpTimestamp> timestampFactory)` — this stays the same since it already takes a factory
- Add `using RobertHodgen.Ntp.Client.Remote;` at top

- [ ] **Step 3: Update OriginTimestampTests**

Replace `OriginTimestamp.Now` references with `OriginTimestamp.FromClock(new MonotonicClock())`.
Replace `OriginTimestamp.SerializableNow` references with `OriginTimestamp.SerializableFromClock(new MonotonicClock())`.

- [ ] **Step 4: Update TransmitTimestampTests**

No `Now` usage exists in current tests, so no changes needed here unless tests reference `NtpTimestamp.Now`.

- [ ] **Step 5: Run tests**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~OriginTimestampTests"`
Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~TransmitTimestampTests"`
Expected: All pass.

- [ ] **Step 6: Commit**

```bash
git add src/Client/Remote/Fields/TransmitTimestamp.cs src/Client/Remote/Fields/OriginTimestamp.cs src/Tests/Client/Remote/Fields/OriginTimestampTests.cs
git commit -m "feat: wire TransmitTimestamp and OriginTimestamp through IMonotonicClock"
```

### Task 4: Update TransmitPacketHeader to accept IMonotonicClock

**Files:**
- Modify: `src/Client/Remote/TransmitPacketHeader.cs`
- Modify: `src/Tests/Client/Remote/TransmitPacketHeaderTests.cs`

**Interfaces:**
- Consumes: `IMonotonicClock`, updated field types
- Produces: `TransmitPacketHeader.CreateNewPacket(IMonotonicClock clock)`

- [ ] **Step 1: Update TransmitPacketHeader**

In `TransmitPacketHeader.cs`:
- Change constructor to accept `IMonotonicClock clock`
- Replace `TransmitTimestamp.Now` with `TransmitTimestamp.FromClock(clock)`
- Add `CreateNewPacket(IMonotonicClock clock)` factory method

```csharp
private TransmitPacketHeader(IMonotonicClock clock)
    : base(
        LeapIndicator.Unknown,
        VersionNumber.Four,
        Mode.Client,
        Stratum.Unsynchronized,
        Poll.MaximumRecommended,
        Precision.Microsecond,
        RootDelay.Zero,
        RootDispersion.Zero,
        ReferenceId.Empty,
        ReferenceTimestamp.Zero,
        OriginTimestamp.Zero,
        ReceiveTimestamp.Zero,
        TransmitTimestamp.FromClock(clock))
{
}

public static Packet<TransmitPacketHeader> CreateNewPacket(IMonotonicClock clock)
{
    return Packet<TransmitPacketHeader>.CreateNewFromHeader(new TransmitPacketHeader(clock));
}
```

- [ ] **Step 2: Update TransmitPacketHeaderTests**

Replace `TransmitPacketHeader.CreateNewPacket()` calls with `TransmitPacketHeader.CreateNewPacket(new MonotonicClock())`.

- [ ] **Step 3: Run tests**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~TransmitPacketHeaderTests"`
Expected: All pass.

- [ ] **Step 4: Commit**

```bash
git add src/Client/Remote/TransmitPacketHeader.cs src/Tests/Client/Remote/TransmitPacketHeaderTests.cs
git commit -m "feat: TransmitPacketHeader.CreateNewPacket now requires IMonotonicClock"
```

### Task 5: Update Client to accept and thread IMonotonicClock

**Files:**
- Modify: `src/Client/Client.cs`
- Modify: `src/Tests/Client/ClientTests.cs`

**Interfaces:**
- Consumes: `IMonotonicClock`, updated `TransmitPacketHeader`
- Produces: `Client` constructor accepts `IMonotonicClock?`, `ReadResponse` accepts `IMonotonicClock`

- [ ] **Step 1: Update Client**

In `Client.cs`:
- Add `IMonotonicClock?` constructor parameter (nullable for backward compat with existing tests)
- Store as field `_clock`
- In `ConnectAsync`, pass clock to `TransmitPacketHeader.CreateNewPacket(_clock)`
- In `ReadResponse(int, Memory<byte>)`, accept `IMonotonicClock` parameter, use `clock.Capture()` instead of `NtpTimestamp.Now`
- In `ReadResponse(SocketReceiveFromResult, Memory<byte>)`, pass clock through

Updated signature:
```csharp
public Client(string server = DefaultServer, IMonotonicClock? clock = null)
    : this(server, Dns.GetHostAddressesAsync, clock) { }

internal Client(string server, Func<string, CancellationToken, Task<IPAddress[]>> getHostAddressesAsync, IMonotonicClock? clock = null)
{
    _host = server;
    _getHostAddressesAsync = getHostAddressesAsync;
    _clock = clock;
}
```

For `ReadResponse`, update to:
```csharp
internal static Packet<ReceivePacketHeader> ReadResponse(int receivedBytes, Memory<byte> buffer, IMonotonicClock clock)
{
    var destinationTimestamp = clock.Capture();
    // ... rest same
}
```

- [ ] **Step 2: Update ClientTests**

The existing tests call `Client.ReadResponse(48, response)` without a clock. Update to pass `new MonotonicClock()`.

- [ ] **Step 3: Run tests**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~ClientTests"`
Expected: All pass.

- [ ] **Step 4: Commit**

```bash
git add src/Client/Client.cs src/Tests/Client/ClientTests.cs
git commit -m "feat: Client threads IMonotonicClock through request/response flow"
```

### Task 6: Update RequestTests to use IMonotonicClock

**Files:**
- Modify: `src/Tests/Client/RequestTests.cs`

**Interfaces:**
- Consumes: Updated `TransmitPacketHeader`

- [ ] **Step 1: Update RequestTests**

Replace `TransmitPacketHeader.CreateNewPacket()` calls with `TransmitPacketHeader.CreateNewPacket(new MonotonicClock())`.

- [ ] **Step 2: Run tests**

Run: `dotnet test ntp-cli.sln --filter "FullyQualifiedName~RequestTests"`
Expected: All pass.

- [ ] **Step 3: Commit**

```bash
git add src/Tests/Client/RequestTests.cs
git commit -m "test: update RequestTests to use MonotonicClock"
```

### Task 7: Update CLI Program.cs to create and inject MonotonicClock

**Files:**
- Modify: `src/Cli/Program.cs`

**Interfaces:**
- Consumes: `MonotonicClock`, updated `Client`

- [ ] **Step 1: Update Program.cs**

In `Program.cs` at line 49, replace:
```csharp
var request = await new Client().ConnectAsync(cancellationToken);
```
with:
```csharp
var clock = new MonotonicClock();
var request = await new Client(clock: clock).ConnectAsync(cancellationToken);
```

Add `using RobertHodgen.Ntp.Client.Remote;` at top.

- [ ] **Step 2: Build the solution**

Run: `dotnet build ntp-cli.sln`
Expected: Clean build.

- [ ] **Step 3: Run all tests**

Run: `dotnet test ntp-cli.sln`
Expected: All pass.

- [ ] **Step 4: Commit**

```bash
git add src/Cli/Program.cs
git commit -m "feat: CLI creates MonotonicClock and injects into Client"
```

### Task 8: Create FakeMonotonicClock for test isolation

**Files:**
- Create: `src/Tests/Client/FakeMonotonicClock.cs`

**Interfaces:**
- Consumes: `IMonotonicClock`
- Produces: `FakeMonotonicClock` with settable `Time` property

- [ ] **Step 1: Create FakeMonotonicClock**

```csharp
// src/Tests/Client/FakeMonotonicClock.cs
namespace Roberthodgen.Ntp.Client.Tests;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class FakeMonotonicClock : IMonotonicClock
{
    public DateTime Time { get; set; } = new(2026, 8, 23, 12, 0, 0, DateTimeKind.Utc);

    public DateTime UtcNow => Time;
    public NtpTimestamp Capture() => NtpTimestamp.FromDateTime(Time);
}
```

- [ ] **Step 2: Commit**

```bash
git add src/Tests/Client/FakeMonotonicClock.cs
git commit -m "test: add FakeMonotonicClock for deterministic testing"
```

### Task 9: Final verification — full test suite and build

**Files:**
- No file changes

- [ ] **Step 1: Restore and build**

Run: `dotnet restore ntp-cli.sln && dotnet build ntp-cli.sln`
Expected: Clean build, no errors.

- [ ] **Step 2: Run all tests**

Run: `dotnet test ntp-cli.sln`
Expected: All tests pass.

- [ ] **Step 3: Commit any remaining changes**

```bash
git status
# Commit anything remaining
```
