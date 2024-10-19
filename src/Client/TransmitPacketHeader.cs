namespace RobertHodgen.Ntp.Client;

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

    public static TransmitPacketHeader CreateNew()
    {
        return new ();
    }
}
