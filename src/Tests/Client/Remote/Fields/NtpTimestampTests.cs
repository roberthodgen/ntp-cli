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
