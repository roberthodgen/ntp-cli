# Request Transmit Timestamp Capture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Capture the client request transmit timestamp at packet encode time so RFC 5905 offset and delay calculations use the closest available approximation of packet departure time.

**Architecture:** The client request still sends `org = 0` and writes client departure time to `xmt`. `TransmitTimestamp` owns a nullable captured value and a single factory based on `IMonotonicClock`; `Encode()` captures once, caches the timestamp, and `Value` throws if read before the packet has been encoded. This makes the response `org == request.xmt` validation use the exact timestamp bytes sent on the wire.

**Tech Stack:** .NET 10, xUnit, Shouldly, RFC 5905 Section 8 timestamp exchange.

**Spec:** User request from 2026-08-24 and `docs/rfc5905.txt` lines 1575-1604.

## Global Constraints

- Follow RFC 5905 Section 8: `T1` is the client request departure timestamp carried in request `xmt` and echoed by the server as response `org`.
- The client request sends `OriginTimestamp.Zero`; do not move the request departure timestamp into request `org`.
- `Encode()` has a protocol-visible side effect for timestamp fields that must be struck at send time; document this in `AGENTS.md`.
- Use TDD: write failing tests before production changes.
- No new NuGet dependencies.

---

### Task 1: Deferred Request Transmit Timestamp

**Files:**
- Modify: `src/Client/Remote/Fields/TransmitTimestamp.cs`
- Modify: `src/Client/Remote/TransmitPacketHeader.cs`
- Test: `src/Tests/Client/Remote/Fields/TransmitTimestampTests.cs`
- Test: `src/Tests/Client/Remote/TransmitPacketHeaderTests.cs`

**Interfaces:**
- Consumes: `IMonotonicClock.Capture(): NtpTimestamp`
- Produces: `TransmitTimestamp.FromClock(IMonotonicClock clock)`, `TransmitTimestamp.Value`, `TransmitTimestamp.Encode()`

- [ ] **Step 1: Write failing deferred-capture tests**

Add tests proving a clock-backed transmit timestamp throws before encode, captures on first encode, and reuses the same cached timestamp after subsequent encodes.

- [ ] **Step 2: Run focused tests and verify they fail**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~TransmitTimestampTests`

Expected: fail because `TransmitTimestamp.Value` is currently available before encode and `Encode()` does not cache a deferred timestamp.

- [ ] **Step 3: Implement minimal deferred capture**

Change `TransmitTimestamp` to store `private NtpTimestamp? _value;` and `private readonly IMonotonicClock? _clock;`. `FromClock(clock)` should create a deferred instance. `Value` should return `_value` or throw `ApplicationException("transmit timestamp accessed before being sent")`. `Encode()` should capture once from `_clock`, cache it, and encode the cached value.

- [ ] **Step 4: Verify focused tests pass**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~TransmitTimestampTests`

Expected: pass.

---

### Task 2: Client Origin Validation Uses Encoded Timestamp

**Files:**
- Modify: `src/Client/Client.cs`
- Test: `src/Tests/Client/Remote/TransmitPacketHeaderTests.cs`

**Interfaces:**
- Consumes: `Packet<TransmitPacketHeader>.Encode()` caches `Header.TransmitTimestamp.Value`
- Produces: `Client.ConnectAsync()` sends bytes and then validates responses using the encoded request timestamp.

- [ ] **Step 1: Write failing packet-level test**

Add a test proving `TransmitPacketHeader.CreateNewPacket(clock).Header.TransmitTimestamp.Value` throws before `packet.Encode()`, then equals the request bytes at offsets 40-47 after encode.

- [ ] **Step 2: Run focused tests and verify failure**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~TransmitPacketHeaderTests`

Expected: fail until deferred transmit timestamp behavior is implemented.

- [ ] **Step 3: Ensure client sends and validates the same encoded timestamp**

In `Client.ConnectAsync()`, keep `var requestPacket = TransmitPacketHeader.CreateNewPacket(_clock);`, call `var requestBytes = requestPacket.Encode();`, send `requestBytes`, and only then pass `requestPacket.Header.TransmitTimestamp` to response validation.

- [ ] **Step 4: Verify focused tests pass**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~TransmitPacketHeaderTests`

Expected: pass.

---

### Task 3: Documentation And Full Verification

**Files:**
- Modify: `AGENTS.md`
- Modify: `src/Client/Remote/Fields/OriginTimestamp.cs`

**Interfaces:**
- Consumes: request timestamp strategy from `AGENTS.md`
- Produces: explicit guidance that `Encode()` may capture send-time timestamps and must be called exactly once per packet send path before reading deferred `Value` fields.

- [ ] **Step 1: Clean up OriginTimestamp factory surface**

Remove the unused `OriginTimestamp.SerializableFromClock` and test-only deferred factory if no tests consume them. Keep `OriginTimestamp.Zero`, `OriginTimestamp.FromClock(IMonotonicClock)`, `OriginTimestamp.Parse(Memory<byte>)`, and `Value` as direct non-deferred behavior because client request `org` remains zero.

- [ ] **Step 2: Document the Encode contract**

Update `AGENTS.md` to state that request `xmt` capture is deferred until `Encode()` and cached, so code must not read `TransmitTimestamp.Value` before encoding and must not re-encode the same request packet for multiple sends.

- [ ] **Step 3: Run focused timestamp tests**

Run: `dotnet test src/Tests/Client/Roberthodgen.Ntp.Client.Tests.csproj --filter FullyQualifiedName~Transmit`

Expected: pass.

- [ ] **Step 4: Run full tests**

Run: `dotnet test ntp-cli.sln`

Expected: pass.
