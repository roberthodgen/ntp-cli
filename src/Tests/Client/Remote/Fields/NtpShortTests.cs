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

    [Fact]
    public void FromTimeSpan_WithLastTickBeforeWholeSecond_DoesNotOverflowFraction()
    {
        var value = TimeSpan.FromTicks(TimeSpan.TicksPerSecond - 1);

        var ntpShort = NtpShort.FromTimeSpan(value);

        ((int)ntpShort.Seconds).ShouldBe(0);
        ((int)ntpShort.Fraction).ShouldBe(65535);
    }

    [Fact]
    public void Zero_HasAllBitsZero()
    {
        ((int)NtpShort.Zero.Seconds).ShouldBe(0);
        ((int)NtpShort.Zero.Fraction).ShouldBe(0);
    }

    [Fact]
    public void Zero_Encode_ProducesFourZeroBytes()
    {
        var bytes = NtpShort.Zero.Encode();

        bytes.Length.ShouldBe(4);
        bytes.ShouldAllBe(b => b == 0);
    }

    [Fact]
    public void Zero_ParseFromFourZeroBytes_ReturnsZero()
    {
        var bytes = new byte[4];
        var parsed = NtpShort.Parse(bytes);

        ((int)parsed.Seconds).ShouldBe(0);
        ((int)parsed.Fraction).ShouldBe(0);
    }

    [Fact]
    public void Zero_RoundTrip_PreservesZero()
    {
        var roundTripped = NtpShort.Parse(NtpShort.Zero.Encode());

        ((int)roundTripped.Seconds).ShouldBe(0);
        ((int)roundTripped.Fraction).ShouldBe(0);
    }

    [Fact]
    public void SizeInBits_IsThirtyTwo()
    {
        NtpShort.Zero.SizeInBits.ShouldBe(32);
    }

    [Fact]
    public void Encode_ProducesNetworkByteOrder()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(300));
        var bytes = ntpShort.Encode();

        bytes.Length.ShouldBe(4);

        var expectedSeconds = ntpShort.Seconds;
        bytes[0].ShouldBe((byte)((expectedSeconds >> 8) & 0xFF));
        bytes[1].ShouldBe((byte)(expectedSeconds & 0xFF));
    }

    [Fact]
    public void Parse_WithNetworkByteOrderSeconds_CorrectlyExtractsValue()
    {
        var seconds = 300;
        var bytes = new byte[4];
        bytes[0] = (byte)((seconds >> 8) & 0xFF);
        bytes[1] = (byte)(seconds & 0xFF);

        var parsed = NtpShort.Parse(bytes);
        ((int)parsed.Seconds).ShouldBe(seconds);
    }

    [Fact]
    public void Parse_WithNetworkByteOrderFraction_CorrectlyExtractsValue()
    {
        var fraction = 32768;
        var bytes = new byte[4];
        bytes[2] = (byte)((fraction >> 8) & 0xFF);
        bytes[3] = (byte)(fraction & 0xFF);

        var parsed = NtpShort.Parse(bytes);
        ((int)parsed.Fraction).ShouldBe(fraction);
    }

    [Fact]
    public void Parse_WithWrongLength_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => NtpShort.Parse(new byte[3]))
            .ParamName.ShouldBe("memory");

        Should.Throw<ArgumentException>(() => NtpShort.Parse(new byte[5]))
            .ParamName.ShouldBe("memory");
    }

    [Fact]
    public void ToTimeSpan_Zero_ReturnsZeroTimeSpan()
    {
        NtpShort.Zero.ToTimeSpan().ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void FromTimeSpan_OneSecond_HasZeroFraction()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(1));

        ((int)ntpShort.Seconds).ShouldBe(1);
        ((int)ntpShort.Fraction).ShouldBe(0);
    }

    [Fact]
    public void Fraction_ResolutionIsTwoToTheNegativeSixteen()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(0.5));

        ntpShort.Fraction.ShouldBe(Convert.ToUInt16(0.5 * 65536d));
    }
}
