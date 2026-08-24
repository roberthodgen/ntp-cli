namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class LeapIndicatorTests
{
    [Fact]
    public void LeapIndicator_Encode_Correct()
    {
        LeapIndicator.NoWarning.Encode()[0].ShouldBe((byte)0b_0000_0000);
        LeapIndicator.LastMinuteOfTheDayHas61Seconds.Encode()[0].ShouldBe((byte)0b_0000_0001);
        LeapIndicator.LastMinuteOfTheDayHas59Seconds.Encode()[0].ShouldBe((byte)0b_0000_0010);
        LeapIndicator.Unknown.Encode()[0].ShouldBe((byte)0b_0000_0011);
    }

    [Fact]
    public void LeapIndicator_SizeInBits_IsTwo()
    {
        LeapIndicator.NoWarning.SizeInBits.ShouldBe(2);
    }

    [Theory]
    [InlineData(0, "No warning")]
    [InlineData(1, "Last minute of the day has 61 seconds")]
    [InlineData(2, "Last minute of the day has 59 seconds")]
    [InlineData(3, "Unknown (clock unsynchronized)")]
    public void ToString_WithValidValue_ReturnsExpectedLabel(byte value, string expected)
    {
        var indicator = LeapIndicator.Reconstitute(value);
        indicator.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Reconstitute_RoundTrip_PreservesValue(byte value)
    {
        var indicator = LeapIndicator.Reconstitute(value);
        indicator.Value.ShouldBe(value);
    }

    [Fact]
    public void NoWarning_HasValueZero()
    {
        ((int)LeapIndicator.NoWarning.Value).ShouldBe(0);
    }

    [Fact]
    public void LastMinuteOfTheDayHas61Seconds_HasValueOne()
    {
        ((int)LeapIndicator.LastMinuteOfTheDayHas61Seconds.Value).ShouldBe(1);
    }

    [Fact]
    public void LastMinuteOfTheDayHas59Seconds_HasValueTwo()
    {
        ((int)LeapIndicator.LastMinuteOfTheDayHas59Seconds.Value).ShouldBe(2);
    }

    [Fact]
    public void Unknown_HasValueThree()
    {
        ((int)LeapIndicator.Unknown.Value).ShouldBe(3);
    }

    [Fact]
    public void ToString_WithOutOfRangeValue_ThrowsApplicationException()
    {
        var indicator = LeapIndicator.Reconstitute(4);
        Should.Throw<ApplicationException>(() => indicator.ToString());
    }
}
