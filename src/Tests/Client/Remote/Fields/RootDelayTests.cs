namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class RootDelayTests
{
    [Fact]
    public void Zero_HasZeroNtpShort()
    {
        ((int)RootDelay.Zero.Value.Seconds).ShouldBe(0);
        ((int)RootDelay.Zero.Value.Fraction).ShouldBe(0);
    }

    [Fact]
    public void SizeInBits_IsThirtyTwo()
    {
        RootDelay.Zero.SizeInBits.ShouldBe(32);
    }

    [Fact]
    public void RoundTrip_PreservesValue()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(100));
        var encoded = ntpShort.Encode();
        var parsed = RootDelay.Parse(encoded);

        ((int)parsed.Value.Seconds).ShouldBe((int)ntpShort.Seconds);
        ((int)parsed.Value.Fraction).ShouldBe((int)ntpShort.Fraction);
    }

    [Fact]
    public void Zero_Encode_ProducesFourZeroBytes()
    {
        var bytes = RootDelay.Zero.Encode();

        bytes.Length.ShouldBe(4);
        bytes.ShouldAllBe(b => b == 0);
    }

    [Fact]
    public void Encode_DelegatesToNtpShort()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(200));
        var rootDelay = RootDelay.Parse(ntpShort.Encode());

        var encoded = rootDelay.Encode();
        var ntpEncoded = ntpShort.Encode();

        encoded.ShouldBe(ntpEncoded);
    }
}
