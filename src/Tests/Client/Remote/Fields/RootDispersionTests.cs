namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class RootDispersionTests
{
    [Fact]
    public void Zero_HasZeroNtpShort()
    {
        ((int)RootDispersion.Zero.Value.Seconds).ShouldBe(0);
        ((int)RootDispersion.Zero.Value.Fraction).ShouldBe(0);
    }

    [Fact]
    public void SizeInBits_IsThirtyTwo()
    {
        RootDispersion.Zero.SizeInBits.ShouldBe(32);
    }

    [Fact]
    public void RoundTrip_PreservesValue()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(100));
        var encoded = ntpShort.Encode();
        var parsed = RootDispersion.Parse(encoded);

        ((int)parsed.Value.Seconds).ShouldBe((int)ntpShort.Seconds);
        ((int)parsed.Value.Fraction).ShouldBe((int)ntpShort.Fraction);
    }

    [Fact]
    public void Zero_Encode_ProducesFourZeroBytes()
    {
        var bytes = RootDispersion.Zero.Encode();

        bytes.Length.ShouldBe(4);
        bytes.ShouldAllBe(b => b == 0);
    }

    [Fact]
    public void Encode_DelegatesToNtpShort()
    {
        var ntpShort = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(200));
        var rootDispersion = RootDispersion.Parse(ntpShort.Encode());

        var encoded = rootDispersion.Encode();
        var ntpEncoded = ntpShort.Encode();

        encoded.ShouldBe(ntpEncoded);
    }
}
