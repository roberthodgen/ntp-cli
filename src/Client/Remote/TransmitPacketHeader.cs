namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// Transmit Packet Header (<c>x.</c>).
/// </summary>
public sealed record TransmitPacketHeader : PacketHeaderBase
{
    private TransmitPacketHeader()
        : base(
            LeapIndicator.Unknown,
            VersionNumber.Four,
            Mode.Client,
            Stratum.Unsynchronized,
            Poll.MaximumRecommended,
            Precision.Microsecond,
            RootDelay.Zero,
            RootDispersion.Zero,
            ReferenceId.Empty,
            ReferenceTimestamp.Zero, // TODO never checked
            OriginTimestamp.Now,
            ReceiveTimestamp.Zero,
            TransmitTimestamp.Zero)
    {
    }

    public static Packet<TransmitPacketHeader> CreateNewPacket()
    {
        return Packet<TransmitPacketHeader>.CreateNewFromHeader(new TransmitPacketHeader());
    }
}
