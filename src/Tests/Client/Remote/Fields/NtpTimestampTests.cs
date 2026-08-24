namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote;
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

    [Fact]
    public void Zero_HasAllBitsZero()
    {
        ((long)NtpTimestamp.Zero.Seconds).ShouldBe(0L);
        ((long)NtpTimestamp.Zero.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void Zero_Encode_ProducesEightZeroBytes()
    {
        var bytes = NtpTimestamp.Zero.Encode();

        bytes.Length.ShouldBe(8);
        bytes.ShouldAllBe(b => b == 0);
    }

    [Fact]
    public void Zero_ParseFromEightZeroBytes_ReturnsZero()
    {
        var bytes = new byte[8];
        var parsed = NtpTimestamp.Parse(bytes);

        ((long)parsed.Seconds).ShouldBe(0L);
        ((long)parsed.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void Zero_RoundTrip_PreservesZero()
    {
        var roundTripped = NtpTimestamp.Parse(NtpTimestamp.Zero.Encode());

        ((long)roundTripped.Seconds).ShouldBe(0L);
        ((long)roundTripped.Fraction).ShouldBe(0L);
    }

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

    [Fact]
    public void SizeInBits_IsSixtyFour()
    {
        NtpTimestamp.Zero.SizeInBits.ShouldBe(64);
    }

    [Fact]
    public void FromDateTime_AtUnixEpoch_ReturnsNtpEpochOffsetSeconds()
    {
        var timestamp = NtpTimestamp.FromDateTime(DateTime.UnixEpoch);

        ((long)timestamp.Seconds).ShouldBe(2208988800L);
        ((long)timestamp.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void ToDateTime_AtUnixEpochOffset_ReturnsJanuaryFirst1970()
    {
        var timestamp = NtpTimestamp.FromDateTime(DateTime.UnixEpoch);
        var dateTime = timestamp.ToDateTime();

        dateTime.ShouldBe(DateTime.UnixEpoch);
    }

    [Fact]
    public void Encode_ProducesNetworkByteOrder()
    {
        var timestamp = NtpTimestamp.FromDateTime(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var bytes = timestamp.Encode();

        bytes.Length.ShouldBe(8);

        var expectedSeconds = timestamp.Seconds;
        ((long)bytes[0]).ShouldBe((expectedSeconds >> 24) & 0xFF);
        ((long)bytes[1]).ShouldBe((expectedSeconds >> 16) & 0xFF);
        ((long)bytes[2]).ShouldBe((expectedSeconds >> 8) & 0xFF);
        ((long)bytes[3]).ShouldBe(expectedSeconds & 0xFF);
    }

    [Fact]
    public void Parse_WithWrongLength_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => NtpTimestamp.Parse(new byte[7]))
            .ParamName.ShouldBe("memory");

        Should.Throw<ArgumentException>(() => NtpTimestamp.Parse(new byte[9]))
            .ParamName.ShouldBe("memory");
    }

    [Fact]
    public void Parse_WithNetworkByteOrderSeconds_CorrectlyExtractsValue()
    {
        var seconds = 2208988800u;
        var bytes = new byte[8];
        bytes[0] = (byte)((seconds >> 24) & 0xFF);
        bytes[1] = (byte)((seconds >> 16) & 0xFF);
        bytes[2] = (byte)((seconds >> 8) & 0xFF);
        bytes[3] = (byte)(seconds & 0xFF);

        var parsed = NtpTimestamp.Parse(bytes);
        ((long)parsed.Seconds).ShouldBe((long)seconds);
    }

    [Fact]
    public void Parse_WithNetworkByteOrderFraction_CorrectlyExtractsValue()
    {
        var fraction = 2147483648u;
        var bytes = new byte[8];
        bytes[4] = (byte)((fraction >> 24) & 0xFF);
        bytes[5] = (byte)((fraction >> 16) & 0xFF);
        bytes[6] = (byte)((fraction >> 8) & 0xFF);
        bytes[7] = (byte)(fraction & 0xFF);

        var parsed = NtpTimestamp.Parse(bytes);
        ((long)parsed.Fraction).ShouldBe((long)fraction);
    }

    [Fact]
    public void Encode_WithKnownSeconds_ProducesExpectedNetworkBytes()
    {
        var timestamp = NtpTimestamp.FromDateTime(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var bytes = timestamp.Encode();

        var expectedSeconds = timestamp.Seconds;
        ((long)bytes[0]).ShouldBe((expectedSeconds >> 24) & 0xFF);
        ((long)bytes[1]).ShouldBe((expectedSeconds >> 16) & 0xFF);
        ((long)bytes[2]).ShouldBe((expectedSeconds >> 8) & 0xFF);
        ((long)bytes[3]).ShouldBe(expectedSeconds & 0xFF);
    }

    [Fact]
    public void FromDateTime_RoundTrip_PreservesTimeWithinOneMicrosecond()
    {
        var original = new DateTime(2026, 6, 15, 18, 30, 45, DateTimeKind.Utc).AddTicks(5_000_000);

        var timestamp = NtpTimestamp.FromDateTime(original);
        var roundTripped = timestamp.ToDateTime();

        (roundTripped - original).Duration().ShouldBeLessThan(TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public void Fraction_OneSecondDifference_HasFullSecondsIncrement()
    {
        var oneSecondLater = NtpTimestamp.FromDateTime(DateTime.UnixEpoch.AddSeconds(1));
        var unixEpoch = NtpTimestamp.FromDateTime(DateTime.UnixEpoch);

        ((long)oneSecondLater.Seconds - (long)unixEpoch.Seconds).ShouldBe(1L);
        ((long)oneSecondLater.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void Fraction_OneHalfSecond_EqualsHalfOfTwoToTheThirtyTwo()
    {
        var timestamp = NtpTimestamp.FromDateTime(DateTime.UnixEpoch.AddSeconds(0.5));

        ((long)timestamp.Fraction).ShouldBe((long)Convert.ToUInt32(0.5 * 4294967296d));
    }
}
