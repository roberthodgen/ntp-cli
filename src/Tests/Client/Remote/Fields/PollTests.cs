namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class PollTests
{
    [Fact]
    public void SizeInBits_IsEight()
    {
        Poll.MinimumRecommended.SizeInBits.ShouldBe(8);
    }

    [Fact]
    public void MinimumRecommended_HasValueSix()
    {
        Poll.MinimumRecommended.Value.ShouldBe((sbyte)6);
    }

    [Fact]
    public void MaximumRecommended_HasValueTen()
    {
        Poll.MaximumRecommended.Value.ShouldBe((sbyte)10);
    }

    [Theory]
    [InlineData((sbyte)6)]
    [InlineData((sbyte)10)]
    [InlineData((sbyte)4)]
    [InlineData((sbyte)17)]
    [InlineData((sbyte)-6)]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)127)]
    [InlineData((sbyte)-128)]
    public void Reconstitute_RoundTrip_PreservesValue(sbyte value)
    {
        var poll = Poll.Reconstitute(value);
        poll.Value.ShouldBe(value);
    }

    [Theory]
    [InlineData((sbyte)6)]
    [InlineData((sbyte)10)]
    [InlineData((sbyte)4)]
    [InlineData((sbyte)-6)]
    [InlineData((sbyte)0)]
    public void Encode_ProducesSingleByteWithCorrectValue(sbyte value)
    {
        var poll = Poll.Reconstitute(value);
        var bytes = poll.Encode();

        bytes.Length.ShouldBe(1);
        ((int)bytes[0]).ShouldBe((byte)value);
    }

    [Fact]
    public void Encode_WithNegativeValue_ProducesTwoComplementByte()
    {
        var poll = Poll.Reconstitute(-6);
        var bytes = poll.Encode();

        ((int)bytes[0]).ShouldBe(unchecked((byte)-6));
    }

    [Theory]
    [InlineData((sbyte)0, "1")]
    [InlineData((sbyte)1, "2")]
    [InlineData((sbyte)6, "64")]
    [InlineData((sbyte)10, "1024")]
    [InlineData((sbyte)17, "131072")]
    public void ToString_WithPositiveValue_ReturnsTwoToThePowerOfValue(sbyte value, string expected)
    {
        Poll.Reconstitute(value).ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData((sbyte)-1, "0.5")]
    [InlineData((sbyte)-6, "0.015625")]
    [InlineData((sbyte)-10, "0.0009765625")]
    public void ToString_WithNegativeValue_ReturnsFractionalSeconds(sbyte value, string expected)
    {
        Poll.Reconstitute(value).ToString().ShouldBe(expected);
    }
}
