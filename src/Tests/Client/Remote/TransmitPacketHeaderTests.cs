namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class TransmitPacketHeaderTests
{
    [Fact]
    public void Encode_WithTransmitPacketHeader_SerializesOriginTimestampAtEncodeTime()
    {
        var first = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);
        var second = new DateTime(2026, 8, 21, 12, 0, 1, DateTimeKind.Utc);
        var values = new Queue<NtpTimestamp>(new[]
        {
            NtpTimestamp.FromDateTime(first),
            NtpTimestamp.FromDateTime(second),
        });

        var originTimestamp = OriginTimestamp.CreateSerializableForTesting(values.Dequeue);

        NtpTimestamp.Parse(originTimestamp.Encode()).ToDateTime().ShouldBe(first);
        NtpTimestamp.Parse(originTimestamp.Encode()).ToDateTime().ShouldBe(second);
    }

    [Fact]
    public void CreateNewPacket_UsesSerializableOriginTimestamp()
    {
        var bytes = TransmitPacketHeader.CreateNewPacket().Encode();

        NtpTimestamp.Parse(bytes.AsMemory(24, 8)).ShouldNotBe(NtpTimestamp.Zero);
    }
}
