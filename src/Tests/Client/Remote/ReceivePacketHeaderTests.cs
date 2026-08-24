namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReceivePacketHeaderTests
{
    [Fact]
    public void Parse_WithWord0Values_ParsesAllFirstWordFields()
    {
        var response = CreateResponse();
        response[0] = 0b_11_100_101;
        response[1] = 2;
        response[2] = 6;
        response[3] = unchecked((byte)-20);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.LeapIndicator.Value.ShouldBe((byte)3);
        packet.Header.VersionNumber.Value.ShouldBe((byte)4);
        packet.Header.Mode.Value.ShouldBe((byte)5);
        packet.Header.Stratum.Value.ShouldBe((byte)2);
        packet.Header.Poll.Value.ShouldBe((sbyte)6);
        packet.Header.Precision.Value.ShouldBe((sbyte)-20);
    }

    [Fact]
    public void Parse_WithBytesAfterHeader_AcceptsPacket()
    {
        var response = new byte[52];
        response[0] = 0b_00_100_100;
        response[1] = 1;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.VersionNumber.Value.ShouldBe((byte)4);
        packet.Header.Mode.Value.ShouldBe((byte)4);
    }

    [Fact]
    public void Parse_WithPacketShorterThanHeader_Throws()
    {
        Should.Throw<ArgumentException>(() => ReceivePacketHeader.Parse(new byte[47], NtpTimestamp.Zero))
            .ParamName.ShouldBe("response");
    }

    private static byte[] CreateResponse()
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        return response;
    }
}
