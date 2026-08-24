namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class TransmitPacketHeaderTests
{
    [Fact]
    public void CreateNewPacket_LeavesOriginTimestampZero()
    {
        var bytes = TransmitPacketHeader.CreateNewPacket(new MonotonicClock()).Encode();

        NtpTimestamp.Parse(bytes.AsMemory(24, 8)).ShouldBe(NtpTimestamp.Zero);
    }

    [Fact]
    public void CreateNewPacket_WritesClientDepartureTimeToTransmitTimestamp()
    {
        var bytes = TransmitPacketHeader.CreateNewPacket(new MonotonicClock()).Encode();

        NtpTimestamp.Parse(bytes.AsMemory(40, 8)).ShouldNotBe(NtpTimestamp.Zero);
    }

    [Fact]
    public void CreateNewPacket_TransmitTimestampValueBeforeEncode_Throws()
    {
        var packet = TransmitPacketHeader.CreateNewPacket(new FakeClock());

        Should.Throw<ApplicationException>(() => packet.Header.TransmitTimestamp.Value)
            .Message.ShouldBe("transmit timestamp accessed before being sent");
    }

    [Fact]
    public void CreateNewPacket_Encode_CachesTransmitTimestampValueSentOnWire()
    {
        var clock = new FakeClock
        {
            Time = new DateTime(2026, 8, 24, 14, 17, 1, 123, DateTimeKind.Utc)
        };
        var packet = TransmitPacketHeader.CreateNewPacket(clock);

        var bytes = packet.Encode();

        var encodedTransmitTimestamp = NtpTimestamp.Parse(bytes.AsMemory(40, 8));
        packet.Header.TransmitTimestamp.Value.ShouldBe(encodedTransmitTimestamp);
    }
}
