namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class OriginTimestampTests
{
    [Fact]
    public void FromClock_ProducesNonZeroTimestamp()
    {
        var now = OriginTimestamp.FromClock(new MonotonicClock());
        ((long)now.Value.Seconds).ShouldNotBe(0L);
    }

    [Fact]
    public void SerializableFromClock_HasZeroValueButDeferredFactory()
    {
        var serializable = OriginTimestamp.SerializableFromClock(new MonotonicClock());
        ((long)serializable.Value.Seconds).ShouldBe(0L);
        serializable.ToString().ShouldContain("deferred");
    }

    [Fact]
    public void SizeInBits_IsSixtyFour()
    {
        var parsed = OriginTimestamp.Parse(new byte[8]);
        parsed.SizeInBits.ShouldBe(64);
    }

    [Fact]
    public void Parse_RoundTrip_PreservesTimestamp()
    {
        var timestamp = NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var encoded = timestamp.Encode();
        var parsed = OriginTimestamp.Parse(encoded);

        ((long)parsed.Value.Seconds).ShouldBe((long)timestamp.Seconds);
        ((long)parsed.Value.Fraction).ShouldBe((long)timestamp.Fraction);
    }

    [Fact]
    public void Encode_WithNonDeferredValue_EncodesValue()
    {
        var timestamp = NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var parsed = OriginTimestamp.Parse(timestamp.Encode());

        var encoded = parsed.Encode();
        var roundTripped = NtpTimestamp.Parse(encoded);

        ((long)roundTripped.Seconds).ShouldBe((long)timestamp.Seconds);
        ((long)roundTripped.Fraction).ShouldBe((long)timestamp.Fraction);
    }

    [Fact]
    public void Encode_WithDeferredFactory_EncodesCurrentTime()
    {
        var before = DateTime.UtcNow;
        var serializable = OriginTimestamp.SerializableFromClock(new MonotonicClock());
        var encoded = serializable.Encode();
        var after = DateTime.UtcNow;

        var decoded = NtpTimestamp.Parse(encoded);
        var dateTime = decoded.ToDateTime();
        dateTime.ShouldBeGreaterThanOrEqualTo(before);
        dateTime.ShouldBeLessThanOrEqualTo(after.AddSeconds(1));
    }
}
