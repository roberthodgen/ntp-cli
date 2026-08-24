# RFC 5905 Compliance Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix all identified RFC 5905 compliance defects in the `src/Client` NTP library and cover the corrected packet, timestamp, and client behavior with focused xUnit tests.

**Architecture:** Protocol field behavior stays in `src/Client/Remote/Fields`, packet header parsing stays in `src/Client/Remote`, and network orchestration stays in `src/Client/Client.cs`. Each task is independently testable and uses the smallest API surface needed to express the RFC behavior.

**Tech Stack:** .NET 10, C#, xUnit, Shouldly, `System.Net`, `System.Net.Sockets`, RFC 5905 NTP packet formats.

**Spec:** `docs/superpowers/specs/2026-08-21-rfc-compliance-design.md`

## Global Constraints

- Fix all 10 issues in `src/Client` and `src/Tests/Client`.
- Do not change the CLI layer (`src/Cli`) unless required for compilation.
- Use `docs/rfc5905.txt` as the local standards reference for NTP protocol behavior.
- Preserve NTP network byte order encoding and decoding behavior.
- Use xUnit + Shouldly following existing test patterns.
- Do not implement NTP authentication, MAC parsing, Autokey, RATE backoff, IPv6 ReferenceId hashing, or extension field parsing beyond accepting response packets longer than 48 bytes.

---

## File Structure

- Modify `src/Client/Remote/Fields/NtpTimestamp.cs`: correct NTP 64-bit fractional second divisor to `2^32`.
- Modify `src/Client/Remote/Fields/NtpShort.cs`: correct NTP short fractional second divisor to `2^16` and avoid integer division in `ToTimeSpan()`.
- Create `src/Tests/Client/Remote/Fields/NtpTimestampTests.cs`: cover timestamp round-trip precision and max-fraction conversion.
- Create `src/Tests/Client/Remote/Fields/NtpShortTests.cs`: cover short-format round-trip precision and max-fraction conversion.
- Modify `src/Client/Remote/Fields/ReferenceId.cs`: parse four ASCII octets without endian reversal.
- Create `src/Tests/Client/Remote/Fields/ReferenceIdTests.cs`: cover `DENY`, `GPS `, `RATE`, and `INIT` parsing.
- Modify `src/Client/Remote/ReceivePacketHeader.cs`: parse word 0 fields from the received packet and accept packets with extension bytes.
- Modify `src/Client/Remote/Fields/LeapIndicator.cs`: add `Reconstitute(byte value)`.
- Modify `src/Client/Remote/Fields/Mode.cs`: add `Reconstitute(byte value)`.
- Modify `src/Client/Remote/Fields/Poll.cs`: add `Reconstitute(sbyte value)` for receive parsing.
- Modify `src/Client/Remote/Fields/Precision.cs`: add `Reconstitute(sbyte value)` for receive parsing.
- Create `src/Tests/Client/Remote/ReceivePacketHeaderTests.cs`: cover word 0 parsing and longer-than-header response parsing.
- Modify `src/Client/Client.cs`: resolve `_host`, validate KoD responses after parsing, and keep response read behavior otherwise unchanged.
- Modify `src/Client/KissCodes.cs`: add well-known KoD helpers used by validation.
- Create `src/Client/NtpKissODeathException.cs`: expose the actionable KoD code that caused the failure.
- Modify `src/Client/Remote/TransmitPacketHeader.cs`: use a serializable current timestamp for request departure time.
- Modify `src/Client/Remote/Fields/OriginTimestamp.cs`: support timestamp capture at `Encode()` time.
- Create `src/Client/Properties/AssemblyInfo.cs`: expose internals to the test project for deterministic DNS and timestamp seam tests.
- Create `src/Tests/Client/ClientTests.cs`: cover configured-host DNS usage and KoD validation through `ReadResponse`.
- Create `src/Tests/Client/Remote/TransmitPacketHeaderTests.cs`: cover encode-time origin timestamp capture.
- Modify `src/Client/Remote/Fields/Stratum.cs`: correct secondary stratum range.
- Modify `src/Client/Remote/Fields/ExtensionField.cs`: rename `FieldTye` to `FieldType`.
- Modify `src/Client/Remote/Packet.cs`: replace the incomplete encode note with an explicit limitation comment for authentication/extension fields.
- Create `src/Tests/Client/Remote/Fields/StratumTests.cs`: cover labels for values 0 through 17.

---

### Task 1: Correct Fractional Timestamp Math

**Files:**
- Modify: `src/Client/Remote/Fields/NtpTimestamp.cs`
- Modify: `src/Client/Remote/Fields/NtpShort.cs`
- Create: `src/Tests/Client/Remote/Fields/NtpTimestampTests.cs`
- Create: `src/Tests/Client/Remote/Fields/NtpShortTests.cs`

**Interfaces:**
- Consumes: existing `NtpTimestamp.FromDateTime(DateTime)`, `NtpTimestamp.Parse(Memory<byte>)`, `NtpTimestamp.ToDateTime()`, `NtpShort.FromTimeSpan(TimeSpan)`, `NtpShort.Parse(Memory<byte>)`, `NtpShort.ToTimeSpan()`.
- Produces: unchanged public signatures with RFC-correct fractional divisors.

- [ ] **Step 1: Write failing `NtpTimestamp` tests**

Create `src/Tests/Client/Remote/Fields/NtpTimestampTests.cs`:

```csharp
namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class NtpTimestampTests
{
    [Fact]
    public void RoundTrip_WithFractionalSecond_PreservesTimeWithinOneMicrosecond()
    {
        var value = new DateTime(2026, 8, 21, 12, 34, 56, DateTimeKind.Utc).AddTicks(1_234_567);

        var parsed = NtpTimestamp.Parse(NtpTimestamp.FromDateTime(value).Encode()).ToDateTime();

        (parsed - value).Duration().ShouldBeLessThan(TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public void FromDateTime_WithNearWholeSecondFraction_UsesTwoToTheThirtySecondDivisor()
    {
        var value = DateTime.UnixEpoch.AddTicks(9_999_999);

        var timestamp = NtpTimestamp.FromDateTime(value);

        timestamp.Fraction.ShouldBe(Convert.ToUInt32(0.9999999d * 4294967296d));
    }
}
```

- [ ] **Step 2: Write failing `NtpShort` tests**

Create `src/Tests/Client/Remote/Fields/NtpShortTests.cs`:

```csharp
namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class NtpShortTests
{
    [Fact]
    public void RoundTrip_WithFractionalSecond_PreservesTimeWithinOneMillisecond()
    {
        var value = TimeSpan.FromSeconds(42.25);

        var parsed = NtpShort.Parse(NtpShort.FromTimeSpan(value).Encode()).ToTimeSpan();

        (parsed - value).Duration().ShouldBeLessThan(TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void ToTimeSpan_WithHalfFraction_ReturnsHalfSecond()
    {
        var value = NtpShort.Parse(new byte[] { 0x00, 0x00, 0x80, 0x00 }).ToTimeSpan();

        value.ShouldBe(TimeSpan.FromSeconds(0.5));
    }

    [Fact]
    public void FromTimeSpan_WithNearWholeSecondFraction_UsesTwoToTheSixteenthDivisor()
    {
        var value = TimeSpan.FromSeconds(12.999);

        var ntpShort = NtpShort.FromTimeSpan(value);

        ntpShort.Fraction.ShouldBe(Convert.ToUInt16(0.999d * 65536d));
    }
}
```

- [ ] **Step 3: Run focused tests and verify they fail**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~NtpTimestampTests`

Expected: FAIL because `NtpTimestamp` still divides by `uint.MaxValue`.

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~NtpShortTests`

Expected: FAIL because `NtpShort.ToTimeSpan()` currently performs integer division and `NtpShort` divides by `ushort.MaxValue`.

- [ ] **Step 4: Implement RFC-correct divisors**

Modify `src/Client/Remote/Fields/NtpTimestamp.cs`:

```csharp
private const double FractionDivisor = 4294967296d;
```

Change `FromDateTime` fraction calculation:

```csharp
var fraction = Convert.ToUInt32((diffFromEpoch.TotalSeconds - seconds) * FractionDivisor);
```

Change `ToDateTime()`:

```csharp
public DateTime ToDateTime() => DateTime.UnixEpoch
    .AddSeconds(Seconds - UnixEpochSecondFromEra0 + (Fraction / FractionDivisor));
```

Modify `src/Client/Remote/Fields/NtpShort.cs`:

```csharp
private const double FractionDivisor = 65536d;
```

Change `FromTimeSpan` fraction calculation:

```csharp
var fraction = Convert.ToUInt16((timeSpan.TotalSeconds - seconds) * FractionDivisor);
```

Change `ToTimeSpan()`:

```csharp
public TimeSpan ToTimeSpan()
{
    return TimeSpan.FromSeconds(Seconds + (Fraction / FractionDivisor));
}
```

- [ ] **Step 5: Run focused tests and verify they pass**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~NtpTimestampTests`

Expected: PASS.

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~NtpShortTests`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Client/Remote/Fields/NtpTimestamp.cs src/Client/Remote/Fields/NtpShort.cs src/Tests/Client/Remote/Fields/NtpTimestampTests.cs src/Tests/Client/Remote/Fields/NtpShortTests.cs
git commit -m "fix: correct NTP fractional timestamp math"
```

---

### Task 2: Preserve ReferenceId ASCII Byte Order

**Files:**
- Modify: `src/Client/Remote/Fields/ReferenceId.cs`
- Create: `src/Tests/Client/Remote/Fields/ReferenceIdTests.cs`

**Interfaces:**
- Consumes: existing `ReferenceId.Parse(Memory<byte>)` and `ReferenceId.Value`.
- Produces: `ReferenceId.Parse` that treats the four octets as ASCII in packet order on every CPU architecture.

- [ ] **Step 1: Write failing parse tests**

Create `src/Tests/Client/Remote/Fields/ReferenceIdTests.cs`:

```csharp
namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using System.Text;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReferenceIdTests
{
    [Theory]
    [InlineData("DENY")]
    [InlineData("GPS ")]
    [InlineData("RATE")]
    [InlineData("INIT")]
    public void Parse_WithAsciiReferenceId_PreservesPacketOrder(string value)
    {
        var parsed = ReferenceId.Parse(Encoding.ASCII.GetBytes(value));

        parsed.Value.ShouldBe(value);
    }
}
```

- [ ] **Step 2: Run focused test and verify it fails on little-endian machines**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~ReferenceIdTests`

Expected: FAIL on common little-endian platforms because `ReferenceId.Parse` reverses bytes.

- [ ] **Step 3: Remove byte reversal**

Modify `src/Client/Remote/Fields/ReferenceId.cs`:

```csharp
public static ReferenceId Parse(Memory<byte> memory)
{
    if (memory.Length != 4)
    {
        throw new ArgumentException("Reference ID must be exactly 4 characters.", nameof(memory));
    }

    return new (Encoding.ASCII.GetString(memory.Span));
}
```

- [ ] **Step 4: Run focused test and verify it passes**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~ReferenceIdTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Client/Remote/Fields/ReferenceId.cs src/Tests/Client/Remote/Fields/ReferenceIdTests.cs
git commit -m "fix: preserve ReferenceId byte order"
```

---

### Task 3: Parse Receive Packet Word 0 And Accept Extension Bytes

**Files:**
- Modify: `src/Client/Remote/ReceivePacketHeader.cs`
- Modify: `src/Client/Remote/Fields/LeapIndicator.cs`
- Modify: `src/Client/Remote/Fields/Mode.cs`
- Modify: `src/Client/Remote/Fields/Poll.cs`
- Modify: `src/Client/Remote/Fields/Precision.cs`
- Create: `src/Tests/Client/Remote/ReceivePacketHeaderTests.cs`

**Interfaces:**
- Consumes: `ReceivePacketHeader.Parse(Memory<byte> response, NtpTimestamp destinationTimestamp)`.
- Produces: parsed `LeapIndicator`, `VersionNumber`, `Mode`, `Stratum`, `Poll`, and `Precision` values from RFC 5905 word 0. Produces `LeapIndicator.Reconstitute(byte)`, `Mode.Reconstitute(byte)`, `Poll.Reconstitute(sbyte)`, and `Precision.Reconstitute(sbyte)`.

- [ ] **Step 1: Write failing receive header tests**

Create `src/Tests/Client/Remote/ReceivePacketHeaderTests.cs`:

```csharp
namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReceivePacketHeaderTests
{
    [Fact]
    public void Parse_WithWord0Values_ParsesAllFirstWordFields()
    {
        var response = CreateResponse();
        response[0] = 0b_11_100_101;
        response[1] = 2;
        response[2] = 6;
        response[3] = unchecked((byte)-20);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.LeapIndicator.Value.ShouldBe((byte)3);
        packet.Header.VersionNumber.Value.ShouldBe((byte)4);
        packet.Header.Mode.Value.ShouldBe((byte)5);
        packet.Header.Stratum.Value.ShouldBe((byte)2);
        packet.Header.Poll.Value.ShouldBe((sbyte)6);
        packet.Header.Precision.Value.ShouldBe((sbyte)-20);
    }

    [Fact]
    public void Parse_WithBytesAfterHeader_AcceptsPacket()
    {
        var response = new byte[52];
        response[0] = 0b_00_100_100;
        response[1] = 1;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.VersionNumber.Value.ShouldBe((byte)4);
        packet.Header.Mode.Value.ShouldBe((byte)4);
    }

    [Fact]
    public void Parse_WithPacketShorterThanHeader_Throws()
    {
        Should.Throw<ArgumentException>(() => ReceivePacketHeader.Parse(new byte[47], NtpTimestamp.Zero))
            .ParamName.ShouldBe("response");
    }

    private static byte[] CreateResponse()
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        return response;
    }
}
```

- [ ] **Step 2: Run focused test and verify it fails**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~ReceivePacketHeaderTests`

Expected: FAIL because receive parsing currently hardcodes first-word fields and rejects packets that are not exactly 48 bytes.

- [ ] **Step 3: Add field reconstitution factories**

Modify `src/Client/Remote/Fields/LeapIndicator.cs`:

```csharp
public static LeapIndicator Reconstitute(byte value) => new (value);
```

Modify `src/Client/Remote/Fields/Mode.cs`:

```csharp
public static Mode Reconstitute(byte value) => new (value);
```

Modify `src/Client/Remote/Fields/Poll.cs`:

```csharp
public static Poll Reconstitute(sbyte value) => new (value);
```

Modify `src/Client/Remote/Fields/Precision.cs`:

```csharp
public static Precision Reconstitute(sbyte value) => new (value);
```

- [ ] **Step 4: Parse word 0 from packet bytes**

Modify `src/Client/Remote/ReceivePacketHeader.cs`:

```csharp
public static Packet<ReceivePacketHeader> Parse(Memory<byte> response, NtpTimestamp destinationTimestamp)
{
    if (response.Length < 48)
    {
        throw new ArgumentException("Header must be at least 48 bytes.", nameof(response));
    }

    var word0Bytes = response[..4].ToArray();
    if (BitConverter.IsLittleEndian)
    {
        Array.Reverse(word0Bytes);
    }

    var word0 = BitConverter.ToUInt32(word0Bytes);
    var leapIndicator = LeapIndicator.Reconstitute((byte)((word0 >> 30) & 0b_11));
    var versionNumber = VersionNumber.Reconstitute((byte)((word0 >> 27) & 0b_111));
    var mode = Mode.Reconstitute((byte)((word0 >> 24) & 0b_111));
    var stratum = Stratum.Reconstitute(response.Span[1]);
    var poll = Poll.Reconstitute(unchecked((sbyte)response.Span[2]));
    var precision = Precision.Reconstitute(unchecked((sbyte)response.Span[3]));

    var rootDelay = RootDelay.Parse(response[4..8]);
    var rootDispersion = RootDispersion.Parse(response[8..12]);
    var referenceId = ReferenceId.Parse(response[12..16]);
    var referenceTimestamp = ReferenceTimestamp.Parse(response[16..24]);
    var originTimestamp = OriginTimestamp.Parse(response[24..32]);
    var receiveTimestamp = ReceiveTimestamp.Parse(response[32..40]);
    var transmitTimestamp = TransmitTimestamp.Parse(response[40..48]);

    return Packet<ReceivePacketHeader>.CreateNewFromHeaderWithDestinationTimestamp(
        new ReceivePacketHeader(
            leapIndicator,
            versionNumber,
            mode,
            stratum,
            poll,
            precision,
            rootDelay,
            rootDispersion,
            referenceId,
            referenceTimestamp,
            originTimestamp,
            receiveTimestamp,
            transmitTimestamp),
        destinationTimestamp);
}
```

The first-word byte offsets are zero-based packet byte offsets: byte 0 contains LI/VN/Mode, byte 1 contains Stratum, byte 2 contains Poll, and byte 3 contains Precision.

- [ ] **Step 5: Run focused test and verify it passes**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~ReceivePacketHeaderTests`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Client/Remote/ReceivePacketHeader.cs src/Client/Remote/Fields/LeapIndicator.cs src/Client/Remote/Fields/Mode.cs src/Client/Remote/Fields/Poll.cs src/Client/Remote/Fields/Precision.cs src/Tests/Client/Remote/ReceivePacketHeaderTests.cs
git commit -m "fix: parse receive packet word zero"
```

---

### Task 4: Fix Client DNS, KoD Handling, And Departure Timestamp Capture

**Files:**
- Modify: `src/Client/Client.cs`
- Modify: `src/Client/KissCodes.cs`
- Create: `src/Client/NtpKissODeathException.cs`
- Modify: `src/Client/Remote/ReceivePacketHeader.cs`
- Modify: `src/Client/Remote/TransmitPacketHeader.cs`
- Modify: `src/Client/Remote/Fields/OriginTimestamp.cs`
- Create: `src/Client/Properties/AssemblyInfo.cs`
- Create: `src/Tests/Client/ClientTests.cs`
- Create: `src/Tests/Client/Remote/TransmitPacketHeaderTests.cs`

**Interfaces:**
- Consumes: parsed `ReceivePacketHeader` from Task 3 and correct `ReferenceId.Parse` from Task 2.
- Produces: `NtpKissODeathException`, `ReceivePacketHeader.KissCode`, `ReceivePacketHeader.ValidateKissODeath()`, `OriginTimestamp.SerializableNow`, and internal test seams for deterministic client and timestamp tests.

- [ ] **Step 1: Expose internals to tests**

Create `src/Client/Properties/AssemblyInfo.cs`:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Roberthodgen.Ntp.Client.Tests")]
```

- [ ] **Step 2: Write failing DNS resolver test**

Create `src/Tests/Client/ClientTests.cs` with this initial content:

```csharp
namespace Roberthodgen.Ntp.Client.Tests;

using System.Net;

public class ClientTests
{
    [Fact]
    public async Task InitializeClientAsync_UsesConfiguredHostForDnsResolution()
    {
        string? resolvedHost = null;
        var client = new Client(
            "time.example.test",
            (host, _) =>
            {
                resolvedHost = host;
                return Task.FromResult(new[] { IPAddress.Loopback });
            });

        await client.InitializeClientAsync();

        resolvedHost.ShouldBe("time.example.test");
    }
}
```

- [ ] **Step 3: Run focused DNS test and verify it fails to compile**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~InitializeClientAsync_UsesConfiguredHostForDnsResolution`

Expected: FAIL to compile because the internal constructor and internal `InitializeClientAsync` seam do not exist yet.

- [ ] **Step 4: Add DNS resolver seam and use `_host`**

Modify `src/Client/Client.cs`:

```csharp
private readonly Func<string, CancellationToken, Task<IPAddress[]>> _getHostAddressesAsync;

public Client(string server = DefaultServer)
    : this(server, Dns.GetHostAddressesAsync)
{
}

internal Client(string server, Func<string, CancellationToken, Task<IPAddress[]>> getHostAddressesAsync)
{
    _host = server;
    _getHostAddressesAsync = getHostAddressesAsync;
}
```

Change `InitializeClientAsync` visibility and resolver call:

```csharp
internal async Task InitializeClientAsync(CancellationToken ct = default)
{
    if (_addresses.Any())
    {
        return;
    }

    Log.Information("Using host: {defaultServer}", _host);
    _addresses.AddRange(await _getHostAddressesAsync(_host, ct));
    if (_addresses.Count == 0)
    {
        throw new ApplicationException("Could not resolve any IP addresses.");
    }

    Log.Debug("Resolved {Count} IPs for host: {IpAddresses}", _addresses.Count, _addresses);
}
```

- [ ] **Step 5: Run focused DNS test and verify it passes**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~InitializeClientAsync_UsesConfiguredHostForDnsResolution`

Expected: PASS.

- [ ] **Step 6: Write failing KoD tests**

Append to `src/Tests/Client/ClientTests.cs`:

```csharp
[Theory]
[InlineData("DENY")]
[InlineData("RSTR")]
[InlineData("RATE")]
public void ReadResponse_WithActionableKissCode_Throws(string kissCode)
{
    var response = CreateKissOfDeathResponse(kissCode);

    var exception = Should.Throw<NtpKissODeathException>(() => Client.ReadResponse(48, response));

    exception.KissCode.Value.ShouldBe(kissCode);
}

[Fact]
public void ReadResponse_WithInformationalKissCode_DoesNotThrow()
{
    var response = CreateKissOfDeathResponse("INIT");

    var packet = Client.ReadResponse(48, response);

    packet.Header.KissCode.ShouldNotBeNull();
    packet.Header.KissCode.Value.ShouldBe("INIT");
}

private static Memory<byte> CreateKissOfDeathResponse(string kissCode)
{
    var response = new byte[48];
    response[0] = 0b_00_100_100;
    response[1] = 0;
    System.Text.Encoding.ASCII.GetBytes(kissCode).CopyTo(response, 12);
    return response;
}
```

- [ ] **Step 7: Run focused KoD tests and verify they fail to compile**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~ReadResponse_`

Expected: FAIL to compile because `NtpKissODeathException`, `Client.ReadResponse(int, Memory<byte>)`, and `ReceivePacketHeader.KissCode` do not exist yet.

- [ ] **Step 8: Add KoD helpers and exception**

Modify `src/Client/KissCodes.cs`:

```csharp
public static KissCodes Deny => new ("DENY");

public static KissCodes RestrictedAccess => new ("RSTR");

public static KissCodes RateExceeded => new ("RATE");

public bool RequiresClientAction => Value is "DENY" or "RSTR" or "RATE";
```

Create `src/Client/NtpKissODeathException.cs`:

```csharp
namespace RobertHodgen.Ntp.Client;

public sealed class NtpKissODeathException : Exception
{
    public KissCodes KissCode { get; }

    public NtpKissODeathException(KissCodes kissCode)
        : base($"Received NTP Kiss-o'-Death packet with kiss code '{kissCode.Value}'.")
    {
        KissCode = kissCode;
    }
}
```

- [ ] **Step 9: Add KoD parsing to receive headers**

Modify `src/Client/Remote/ReceivePacketHeader.cs` by adding these members inside `ReceivePacketHeader`:

```csharp
public KissCodes? KissCode => Stratum == Stratum.UnspecifiedOrInvalid
    ? KissCodes.CreateNew(ReferenceId.Value)
    : null;

public void ValidateKissODeath()
{
    if (KissCode is { RequiresClientAction: true } kissCode)
    {
        throw new NtpKissODeathException(kissCode);
    }
}
```

Add the namespace import at the top of `src/Client/Remote/ReceivePacketHeader.cs`:

```csharp
using RobertHodgen.Ntp.Client;
```

This places the KoD property on `ReceivePacketHeader` rather than `Packet<ReceivePacketHeader>` because C# cannot add members to a single closed generic instantiation without an extension method. Callers still access it from the parsed receive packet via `packet.Header.KissCode`.

- [ ] **Step 10: Validate KoD responses after parsing**

Modify `src/Client/Client.cs`:

```csharp
internal static Packet<ReceivePacketHeader> ReadResponse(int receivedBytes, Memory<byte> buffer)
{
    var destinationTimestamp = NtpTimestamp.Now;
    var actualReceived = buffer[..receivedBytes];
    var receivePacket = ReceivePacketHeader.Parse(actualReceived, destinationTimestamp);
    receivePacket.Header.ValidateKissODeath();
    return receivePacket;
}
```

Change the socket result wrapper to call the new overload:

```csharp
private static Packet<ReceivePacketHeader> ReadResponse(SocketReceiveFromResult response, Memory<byte> buffer)
{
    return ReadResponse(response.ReceivedBytes, buffer);
}
```

- [ ] **Step 11: Run focused KoD tests and verify they pass**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~ReadResponse_`

Expected: PASS.

- [ ] **Step 12: Write failing origin timestamp encode-time test**

Create `src/Tests/Client/Remote/TransmitPacketHeaderTests.cs`:

```csharp
namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class TransmitPacketHeaderTests
{
    [Fact]
    public void Encode_WithTransmitPacketHeader_SerializesOriginTimestampAtEncodeTime()
    {
        var first = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);
        var second = new DateTime(2026, 8, 21, 12, 0, 1, DateTimeKind.Utc);
        var values = new Queue<NtpTimestamp>(new[]
        {
            NtpTimestamp.FromDateTime(first),
            NtpTimestamp.FromDateTime(second),
        });

        var originTimestamp = OriginTimestamp.CreateSerializableForTesting(values.Dequeue);

        NtpTimestamp.Parse(originTimestamp.Encode()).ToDateTime().ShouldBe(first);
        NtpTimestamp.Parse(originTimestamp.Encode()).ToDateTime().ShouldBe(second);
    }

    [Fact]
    public void CreateNewPacket_UsesSerializableOriginTimestamp()
    {
        var bytes = TransmitPacketHeader.CreateNewPacket().Encode();

        NtpTimestamp.Parse(bytes.AsMemory(24, 8)).ShouldNotBe(NtpTimestamp.Zero);
    }
}
```

- [ ] **Step 13: Run focused origin timestamp tests and verify they fail to compile**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~TransmitPacketHeaderTests`

Expected: FAIL to compile because `OriginTimestamp.CreateSerializableForTesting` and `OriginTimestamp.SerializableNow` do not exist yet.

- [ ] **Step 14: Implement encode-time timestamp capture**

Modify `src/Client/Remote/Fields/OriginTimestamp.cs`:

```csharp
public static OriginTimestamp Now => new (NtpTimestamp.Now);

public static OriginTimestamp SerializableNow => new (NtpTimestamp.Zero, () => NtpTimestamp.Now);

internal static OriginTimestamp CreateSerializableForTesting(Func<NtpTimestamp> timestampFactory) =>
    new (NtpTimestamp.Zero, timestampFactory);

public NtpTimestamp Value { get; }

private readonly Func<NtpTimestamp>? _timestampFactory;

public override int SizeInBits => Value.SizeInBits;

private OriginTimestamp(NtpTimestamp value, Func<NtpTimestamp>? timestampFactory = null)
{
    Value = value;
    _timestampFactory = timestampFactory;
}

public static OriginTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

public override byte[] Encode() => (_timestampFactory?.Invoke() ?? Value).Encode();
```

Modify `src/Client/Remote/TransmitPacketHeader.cs`:

```csharp
OriginTimestamp.SerializableNow,
```

This line replaces the existing `OriginTimestamp.Now` constructor argument.

- [ ] **Step 15: Run focused origin timestamp tests and verify they pass**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~TransmitPacketHeaderTests`

Expected: PASS.

- [ ] **Step 16: Run all Task 4 tests**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter "FullyQualifiedName~ClientTests|FullyQualifiedName~TransmitPacketHeaderTests"`

Expected: PASS.

- [ ] **Step 17: Commit**

```bash
git add src/Client/Client.cs src/Client/KissCodes.cs src/Client/NtpKissODeathException.cs src/Client/Properties/AssemblyInfo.cs src/Client/Remote/ReceivePacketHeader.cs src/Client/Remote/TransmitPacketHeader.cs src/Client/Remote/Fields/OriginTimestamp.cs src/Tests/Client/ClientTests.cs src/Tests/Client/Remote/TransmitPacketHeaderTests.cs
git commit -m "fix: handle KoD and client request timing"
```

---

### Task 5: Apply Minor RFC Cleanup And Final Verification

**Files:**
- Modify: `src/Client/Remote/Fields/Stratum.cs`
- Modify: `src/Client/Remote/Fields/ExtensionField.cs`
- Modify: `src/Client/Remote/Packet.cs`
- Create: `src/Tests/Client/Remote/Fields/StratumTests.cs`

**Interfaces:**
- Consumes: existing `Stratum.Reconstitute(byte)`, `Stratum.ToString()`, `ExtensionField.FieldTye`, and `Packet<THeader>.Encode()`.
- Produces: correct stratum labels for 2-15, renamed `ExtensionField.FieldType`, and an explicit encode limitation comment for authentication/extension fields.

- [ ] **Step 1: Write failing stratum tests**

Create `src/Tests/Client/Remote/Fields/StratumTests.cs`:

```csharp
namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class StratumTests
{
    [Theory]
    [InlineData(0, "Unspecified or Invalid")]
    [InlineData(1, "Primary Server (e.g., equipped with a GPS receiver)")]
    [InlineData(2, "Secondary Server (via NTP)")]
    [InlineData(15, "Secondary Server (via NTP)")]
    [InlineData(16, "Unsynchronized")]
    [InlineData(17, "Reserved")]
    public void ToString_WithKnownRange_ReturnsExpectedLabel(byte value, string expected)
    {
        Stratum.Reconstitute(value).ToString().ShouldBe(expected);
    }
}
```

- [ ] **Step 2: Run focused stratum test and verify it fails**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~StratumTests`

Expected: FAIL because stratum 2 currently returns `Reserved`.

- [ ] **Step 3: Fix stratum secondary range**

Modify `src/Client/Remote/Fields/Stratum.cs`:

```csharp
public override string ToString() => Value switch
{
    0 => "Unspecified or Invalid",
    1 => "Primary Server (e.g., equipped with a GPS receiver)",
    >= 2 and <= 15 => "Secondary Server (via NTP)",
    16 => "Unsynchronized",
    _ => "Reserved",
};
```

- [ ] **Step 4: Rename extension field property**

Modify `src/Client/Remote/Fields/ExtensionField.cs`:

```csharp
public ushort FieldType { get; }
```

This is a straight rename from `FieldTye`. No call sites currently reference the misspelled property.

- [ ] **Step 5: Clarify packet encode limitation**

Modify `src/Client/Remote/Packet.cs`:

```csharp
// This client does not implement NTP authentication or extension field serialization.
// Requests are unauthenticated 48-byte headers; received extension bytes are accepted but not re-encoded.
public byte[] Encode() => Header.Encode();
```

- [ ] **Step 6: Run focused stratum test and verify it passes**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~StratumTests`

Expected: PASS.

- [ ] **Step 7: Run full solution verification**

Run: `dotnet restore ntp-cli.sln`

Expected: succeeds with no restore errors.

Run: `dotnet build ntp-cli.sln`

Expected: succeeds with no compiler errors.

Run: `dotnet test ntp-cli.sln`

Expected: all tests pass.

- [ ] **Step 8: Commit**

```bash
git add src/Client/Remote/Fields/Stratum.cs src/Client/Remote/Fields/ExtensionField.cs src/Client/Remote/Packet.cs src/Tests/Client/Remote/Fields/StratumTests.cs
git commit -m "fix: clean up RFC compliance edge cases"
```

---

## Final Review Checklist

- [ ] Verify `ReceivePacketHeader.Parse` accepts response packets where `response.Length >= 48` and rejects shorter packets.
- [ ] Verify first-word parsing uses RFC 5905 packet byte order: byte 0 for LI/VN/Mode, byte 1 for Stratum, byte 2 for Poll, byte 3 for Precision.
- [ ] Verify `ReferenceId.Parse` preserves ASCII order for all tested kiss/reference codes.
- [ ] Verify KoD `DENY`, `RSTR`, and `RATE` throw `NtpKissODeathException` and `INIT` does not.
- [ ] Verify `Client.InitializeClientAsync` resolves `_host`, not the default server literal.
- [ ] Verify unauthenticated packet encode behavior remains a 48-byte header.
- [ ] Verify `dotnet test ntp-cli.sln` passes before reporting completion.
