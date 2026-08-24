namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using System.Text;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReferenceIdTests
{
    [Theory]
    [InlineData("DENY")]
    [InlineData("GPS ")]
    [InlineData("RATE")]
    [InlineData("INIT")]
    public void Parse_WithAsciiReferenceId_PreservesPacketOrder(string value)
    {
        var parsed = ReferenceId.Parse(Encoding.ASCII.GetBytes(value));

        parsed.Value.ShouldBe(value);
    }
}
