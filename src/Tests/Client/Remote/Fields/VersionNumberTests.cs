namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class VersionNumberTests
{
    [Fact]
    public void SizeInBits_IsThree()
    {
        VersionNumber.Four.SizeInBits.ShouldBe(3);
    }

    [Fact]
    public void Four_HasValueFour()
    {
        ((int)VersionNumber.Four.Value).ShouldBe(4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Reconstitute_RoundTrip_PreservesValue(byte value)
    {
        var version = VersionNumber.Reconstitute(value);
        ((int)version.Value).ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Encode_ProducesSingleByteWithCorrectValue(byte value)
    {
        var version = VersionNumber.Reconstitute(value);
        var bytes = version.Encode();

        bytes.Length.ShouldBe(1);
        ((int)bytes[0]).ShouldBe(value);
    }

    [Theory]
    [InlineData(0, "v0")]
    [InlineData(1, "v1")]
    [InlineData(2, "v2")]
    [InlineData(3, "v3")]
    [InlineData(4, "v4")]
    [InlineData(5, "v5")]
    [InlineData(6, "v6")]
    [InlineData(7, "v7")]
    public void ToString_PrefixesValueWithV(byte value, string expected)
    {
        VersionNumber.Reconstitute(value).ToString().ShouldBe(expected);
    }
}
