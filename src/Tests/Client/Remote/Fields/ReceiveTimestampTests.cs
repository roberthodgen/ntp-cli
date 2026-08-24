namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReceiveTimestampTests
{
    [Fact]
    public void Zero_HasZeroNtpTimestamp()
    {
        ((long)ReceiveTimestamp.Zero.Value.Seconds).ShouldBe(0L);
        ((long)ReceiveTimestamp.Zero.Value.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void SizeInBits_IsSixtyFour()
    {
        ReceiveTimestamp.Zero.SizeInBits.ShouldBe(64);
    }

    [Fact]
    public void RoundTrip_PreservesTimestamp()
    {
        var timestamp = NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var encoded = timestamp.Encode();
        var parsed = ReceiveTimestamp.Parse(encoded);

        ((long)parsed.Value.Seconds).ShouldBe((long)timestamp.Seconds);
        ((long)parsed.Value.Fraction).ShouldBe((long)timestamp.Fraction);
    }

    [Fact]
    public void Zero_Encode_ProducesEightZeroBytes()
    {
        var bytes = ReceiveTimestamp.Zero.Encode();

        bytes.Length.ShouldBe(8);
        bytes.ShouldAllBe(b => b == 0);
    }
}
