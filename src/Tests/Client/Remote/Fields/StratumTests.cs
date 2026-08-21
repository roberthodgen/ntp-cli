namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class StratumTests
{
    [Theory]
    [InlineData(0, "Unspecified or Invalid")]
    [InlineData(1, "Primary Server (e.g., equipped with a GPS receiver)")]
    [InlineData(2, "Secondary Server (via NTP)")]
    [InlineData(15, "Secondary Server (via NTP)")]
    [InlineData(16, "Unsynchronized")]
    [InlineData(17, "Reserved")]
    public void ToString_WithKnownRange_ReturnsExpectedLabel(byte value, string expected)
    {
        Stratum.Reconstitute(value).ToString().ShouldBe(expected);
    }
}