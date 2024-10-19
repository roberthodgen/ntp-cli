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
}
