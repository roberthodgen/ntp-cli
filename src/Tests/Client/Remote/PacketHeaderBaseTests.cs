namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class PacketHeaderBaseTests
{
    [Fact]
    public void Encode_ProducesFortyEightBytes()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        bytes.Length.ShouldBe(48);
    }

    [Fact]
    public void Encode_Word0ContainsPackedLiVnAndMode()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        var li = (bytes[0] >> 6) & 0b_11;
        var vn = (bytes[0] >> 3) & 0b_111;
        var mode = bytes[0] & 0b_111;

        li.ShouldBe(3);
        vn.ShouldBe(4);
        mode.ShouldBe(3);
    }

    [Fact]
    public void Encode_StratumIsAtByte1()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        ((int)bytes[1]).ShouldBe(16);
    }

    [Fact]
    public void Encode_PollIsAtByte2()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        ((int)bytes[2]).ShouldBe(10);
    }

    [Fact]
    public void Encode_PrecisionIsAtByte3()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        ((int)bytes[3]).ShouldBe(unchecked((byte)-18));
    }

    [Fact]
    public void Encode_RootDelayIsAtBytes4Through7()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        ((int)bytes[4]).ShouldBe(0);
        ((int)bytes[5]).ShouldBe(0);
        ((int)bytes[6]).ShouldBe(0);
        ((int)bytes[7]).ShouldBe(0);
    }

    [Fact]
    public void Encode_RootDispersionIsAtBytes8Through11()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        ((int)bytes[8]).ShouldBe(0);
        ((int)bytes[9]).ShouldBe(0);
        ((int)bytes[10]).ShouldBe(0);
        ((int)bytes[11]).ShouldBe(0);
    }

    [Fact]
    public void Encode_ReferenceIdIsAtBytes12Through15()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        var refId = System.Text.Encoding.ASCII.GetString(bytes[12..16]);
        refId.ShouldBe("    ");
    }

    [Fact]
    public void Encode_ReferenceTimestampIsAtBytes16Through23()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        var timestamp = NtpTimestamp.Parse(bytes[16..24]);
        ((long)timestamp.Seconds).ShouldBe(0L);
        ((long)timestamp.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void Encode_OriginTimestampIsAtBytes24Through31()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        var timestamp = NtpTimestamp.Parse(bytes[24..32]);
        ((long)timestamp.Seconds).ShouldBe(0L);
        ((long)timestamp.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void Encode_ReceiveTimestampIsAtBytes32Through39()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        var timestamp = NtpTimestamp.Parse(bytes[32..40]);
        ((long)timestamp.Seconds).ShouldBe(0L);
        ((long)timestamp.Fraction).ShouldBe(0L);
    }

    [Fact]
    public void Encode_TransmitTimestampIsAtBytes40Through47()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        var bytes = header.Encode();

        var timestamp = NtpTimestamp.Parse(bytes[40..48]);
        ((long)timestamp.Seconds).ShouldNotBe(0L);
    }

    [Fact]
    public void KissODeath_WithNonZeroStratum_ReturnsFalse()
    {
        var header = TransmitPacketHeader.CreateNewPacket().Header;
        header.KissODeath.ShouldBeFalse();
    }
}
