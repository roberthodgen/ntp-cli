namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using Roberthodgen.Ntp.Client.Tests;
using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class TransmitTimestampTests
{
    [Fact]
    public void Zero_HasZeroNtpTimestamp()
    {
        ((long)TransmitTimestamp.Zero.Value.Seconds).ShouldBe(0L);
        ((long)TransmitTimestamp.Zero.Value.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void SizeInBits_IsSixtyFour()
    {
        TransmitTimestamp.Zero.SizeInBits.ShouldBe(64);
    }

    [Fact]
    public void RoundTrip_PreservesTimestamp()
    {
        var timestamp = NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var encoded = timestamp.Encode();
        var parsed = TransmitTimestamp.Parse(encoded);

        ((long)parsed.Value.Seconds).ShouldBe((long)timestamp.Seconds);
        ((long)parsed.Value.Fraction).ShouldBe((long)timestamp.Fraction);
    }

    [Fact]
    public void Zero_Encode_ProducesEightZeroBytes()
    {
        var bytes = TransmitTimestamp.Zero.Encode();

        bytes.Length.ShouldBe(8);
        bytes.ShouldAllBe(b => b == 0);
    }

    [Fact]
    public void FromClock_ValueBeforeEncode_Throws()
    {
        var timestamp = TransmitTimestamp.FromClock(new FakeClock());

        Should.Throw<ApplicationException>(() => timestamp.Value)
            .Message.ShouldBe("transmit timestamp accessed before being sent");
    }

    [Fact]
    public void FromClock_Encode_CapturesTimestampFromClock()
    {
        var clock = new FakeClock
        {
            Time = new DateTime(2026, 8, 24, 14, 17, 1, 123, DateTimeKind.Utc)
        };
        var timestamp = TransmitTimestamp.FromClock(clock);

        var bytes = timestamp.Encode();

        var expected = NtpTimestamp.FromDateTime(clock.Time);
        NtpTimestamp.Parse(bytes).ShouldBe(expected);
        timestamp.Value.ShouldBe(expected);
    }

    [Fact]
    public void FromClock_EncodeTwice_ReusesFirstCapturedTimestamp()
    {
        var clock = new FakeClock
        {
            Time = new DateTime(2026, 8, 24, 14, 17, 1, DateTimeKind.Utc)
        };
        var timestamp = TransmitTimestamp.FromClock(clock);

        var firstBytes = timestamp.Encode();
        clock.Time = clock.Time.AddSeconds(1);
        var secondBytes = timestamp.Encode();

        secondBytes.ShouldBe(firstBytes);
        timestamp.Value.ShouldBe(NtpTimestamp.Parse(firstBytes));
    }
}
