namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class PacketTests
{
    [Fact]
    public void CreateNewFromHeader_SetsHeader()
    {
        var header = TransmitPacketHeader.CreateNewPacket(new MonotonicClock()).Header;
        var packet = Packet<TransmitPacketHeader>.CreateNewFromHeader(header);

        packet.Header.ShouldNotBeNull();
    }

    [Fact]
    public void CreateNewFromHeader_DestinationTimestampIsNull()
    {
        var header = TransmitPacketHeader.CreateNewPacket(new MonotonicClock()).Header;
        var packet = Packet<TransmitPacketHeader>.CreateNewFromHeader(header);

        packet.DestinationTimestamp.ShouldBeNull();
    }

    [Fact]
    public void CreateNewFromHeaderWithDestinationTimestamp_SetsDestinationTimestamp()
    {
        var header = TransmitPacketHeader.CreateNewPacket(new MonotonicClock()).Header;
        var destTime = NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var packet = Packet<TransmitPacketHeader>.CreateNewFromHeaderWithDestinationTimestamp(header, destTime);

        packet.DestinationTimestamp.ShouldNotBeNull();
        ((long)packet.DestinationTimestamp!.Seconds).ShouldBe((long)destTime.Seconds);
    }

    [Fact]
    public void KeyId_DefaultIsNone()
    {
        var packet = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        ((long)packet.KeyId.Value).ShouldBe(0L);
    }

    [Fact]
    public void MessageDigest_DefaultIsNone()
    {
        var packet = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        packet.MessageDigest.Value.Length.ShouldBe(0);
    }

    [Fact]
    public void Extensions_DefaultIsEmpty()
    {
        var packet = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        packet.Extensions.ShouldBeEmpty();
    }

    [Fact]
    public void Encode_ProducesSameLengthAsHeader()
    {
        var packet = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        var headerBytes = packet.Header.Encode();
        var packetBytes = packet.Encode();

        packetBytes.Length.ShouldBe(headerBytes.Length);
    }
}
