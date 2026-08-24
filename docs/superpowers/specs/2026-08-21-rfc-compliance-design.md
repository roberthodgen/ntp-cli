# RFC 5905 Compliance Remediation

**Date:** 2026-08-21
**Status:** Approved

## Problem

The `src/Client` NTP library has 10 issues where its packet parsing, timestamp arithmetic, and client behavior deviate from RFC 5905. Three are critical (silent data corruption), two are high (broken configuration, rejected valid packets), two are medium (missing protocol-required behavior), and three are low (range gap, typo, incomplete encode).

## Scope

Fix all 10 issues in `src/Client` and `src/Tests/Client`. No changes to the CLI layer (`src/Cli`) unless required for compilation.

## Issues

| # | Severity | Description |
|---|----------|-------------|
| 1 | Critical | `ReceivePacketHeader.Parse` hardcodes word 0 fields |
| 2 | Critical | `ReferenceId.Parse` reverses ASCII bytes on little-endian |
| 3 | Critical | `NtpTimestamp` / `NtpShort` fraction uses `MaxValue` instead of `2^p` |
| 4 | High | `Client.InitializeClientAsync` hardcodes `"pool.ntp.org"` |
| 5 | High | `ReceivePacketHeader.Parse` rejects packets longer than 48 bytes |
| 6 | Medium | KoD packets not acted upon per RFC §7.4 |
| 7 | Medium | `OriginTimestamp` captured before send, not at transmission time |
| 8 | Low | `Stratum.ToString()` range gap for values 2 and 16 |
| 9 | Low | `ExtensionField.FieldTye` typo |
| 10 | Low | `Packet.Encode()` doesn't include KeyId, MessageDigest, or Extensions |

## Design

### Area 1: Parse word 0 from receive packets (Issues #1, #5)

**ReceivePacketHeader.Parse** currently reads `word0` but discards it, hardcoding `LeapIndicator`, `VersionNumber`, `Mode`, `Stratum`, `Poll`, and `Precision`. Per RFC §7.3, word 0 layout is:

```
0                   1
0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|LI | VN  |Mode |    Stratum     |     Poll      |  Precision   |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
```

**Changes:**
- Extract `LeapIndicator` (bits 31-30), `VersionNumber` (bits 29-27), `Mode` (bits 26-24) from `word0` using bit shifts and masks
- Extract `Stratum` (byte 2), `Poll` (byte 3), `Precision` (byte 4) directly from the response memory
- Add `Reconstitute`-style factory methods to `LeapIndicator`, `VersionNumber`, and `Mode` (they already have `Reconstitute` on `VersionNumber`, need it on the others)
- Change length check from `response.Length != 48` to `response.Length < 48` to accept extension fields

**Files:** `ReceivePacketHeader.cs`, `LeapIndicator.cs`, `VersionNumber.cs`, `Mode.cs`

### Area 2: Fix ReferenceId endianness (Issue #2)

`ReferenceId.Parse` reverses the 4-byte value on little-endian machines. Per RFC §7.3, ReferenceId is a four-character ASCII string — ASCII has no endianness. The reversal corrupts `"DENY"` to `"Y NED"`, `"GPS "` to `" SP G"`, etc.

**Changes:**
- Remove the `BitConverter.IsLittleEndian` byte reversal from `ReferenceId.Parse`
- `ReferenceId.Encode` is already correct (no reversal) — no change needed

**Files:** `ReferenceId.cs`

### Area 3: Fix timestamp fraction divisors (Issue #3)

`NtpTimestamp` uses `uint.MaxValue` (4,294,967,295) instead of `2^32` (4,294,967,296). `NtpShort` uses `ushort.MaxValue` (65,535) instead of `2^16` (65,536). Per RFC §6, the fraction resolves to `2^(-p)` seconds where `p` is the number of fraction bits. Using `MaxValue` introduces systematic bias.

**Changes:**
- `NtpTimestamp.FromDateTime`: change `uint.MaxValue` to `Math.Pow(2, 32)` (as double)
- `NtpTimestamp.ToDateTime`: change `uint.MaxValue` to `Math.Pow(2, 32)`
- `NtpShort.FromTimeSpan`: change `ushort.MaxValue` to `Math.Pow(2, 16)`
- `NtpShort.ToTimeSpan`: change `ushort.MaxValue` to `Math.Pow(2, 16)`

**Files:** `NtpTimestamp.cs`, `NtpShort.cs`

### Area 4: Fix client behavior (Issues #4, #6, #7)

**Issue #4 — Hardcoded server:** One-line fix. Change `Dns.GetHostAddressesAsync("pool.ntp.org", ct)` to `Dns.GetHostAddressesAsync(_host, ct)`.

**Issue #6 — KoD handling:** After word 0 is parsed (Area 1), detect KoD packets (stratum 0 + valid 4-char ASCII ReferenceId). On `DENY` or `RSTR`, throw a descriptive exception. On `RATE`, throw a descriptive exception. Other KoD codes are informational — no exception needed.

Implementation: Add a `KissCode` property to `Packet<ReceivePacketHeader>` that parses the ReferenceId as a kiss code when `Stratum == 0`. Add a `ValidateKissODeath()` method that throws `NtpKissODeathException` for `DENY`/`RSTR`/`RATE`. Call this after parsing in `Client.ReadResponse`.

Merge `KissCodes` into the parsing flow — the existing `KissCodes` record has a TODO noting this. Keep the record for the exception type.

**Issue #7 — OriginTimestamp timing:** The RFC §8 says timestamps should be struck "upon the arrival or departure of a packet." Currently `OriginTimestamp.Now` is set in the `TransmitPacketHeader` constructor, before `SendToAsync`.

Approach: Introduce `OriginTimestamp.SerializableNow` — a factory that returns an `OriginTimestamp` whose `Encode()` strikes `NtpTimestamp.Now` at call time (as close to the wire as possible). `TransmitPacketHeader` uses `OriginTimestamp.SerializableNow` instead of `OriginTimestamp.Now`.

**Files:** `Client.cs`, `KissCodes.cs`, `TransmitPacketHeader.cs`, `OriginTimestamp.cs`, new `NtpKissODeathException.cs`

### Area 5: Minor fixes (Issues #8, #9, #10)

**Issue #8:** Change `Stratum.ToString()` from `> 2 and < 16` to `>= 2 and <= 15`.

**Issue #9:** Rename `ExtensionField.FieldTye` to `FieldType`.

**Issue #10:** `Packet.Encode()` is incomplete for MAC/extension fields. For a CLI client that doesn't use authentication, this is acceptable. Add a comment clarifying the limitation. No code change needed.

**Files:** `Stratum.cs`, `ExtensionField.cs`

## Execution Order

1. Area 3 (timestamp divisors) — standalone, no dependencies
2. Area 2 (ReferenceId endianness) — standalone
3. Area 1 (word 0 parsing) — standalone
4. Area 4 (client behavior) — depends on Areas 1, 2
5. Area 5 (minor fixes) — standalone

## Testing

xUnit + Shouldly, following existing patterns. Key test scenarios:

- Parse a 48-byte server response with known word 0 values; verify all 6 fields
- Parse `"DENY"`, `"GPS "`, `"RATE"`, `"INIT"` ReferenceIds; verify no byte reversal
- Round-trip `NtpTimestamp` through `FromDateTime` → `Encode` → `Parse` → `ToDateTime`; verify sub-microsecond accuracy
- Round-trip `NtpShort` through `FromTimeSpan` → `Encode` → `Parse` → `ToTimeSpan`
- KoD detection throws `NtpKissODeathException` for `DENY`/`RSTR`/`RATE`
- Client uses configured hostname for DNS resolution
- `Stratum.ToString()` returns correct labels for values 0-16

## Out of Scope

- Implementing NTP authentication (MAC, Autokey)
- Implementing RATE backoff logic (exception thrown; CLI layer handles)
- IPv6 ReferenceId handling (MD5 hash of IPv6 address)
- Extension field parsing beyond accepting them
