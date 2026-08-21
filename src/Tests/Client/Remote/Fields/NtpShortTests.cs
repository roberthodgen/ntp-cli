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