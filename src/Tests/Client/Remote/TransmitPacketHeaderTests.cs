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
}
