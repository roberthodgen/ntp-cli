namespace RobertHodgen.Ntp.Client;

using System.Net;

/// <summary>
/// Transmit Packet Header (<c>x.</c>).
/// </summary>
public sealed record TransmitPacketHeader : PacketHeaderBase
{
    private TransmitPacketHeader()
    {
        LeapIndicator = LeapIndicator.Unknown;
        VersionNumber = VersionNumber.Four;
        Mode = Mode.Client;
        Stratum = Stratum.Unsynchronized;
        Poll = Poll.MaximumRecommended;
        Precision = Precision.Microsecond;
        RootDelay = RootDelay.Zero;
        RootDispersion = RootDispersion.Zero;
        ReferenceId = ReferenceId.Empty;
        ReferenceTimestamp = ReferenceTimestamp.Zero; // TODO never checked
        OriginTimestamp = OriginTimestamp.Now;
        ReceiveTimestamp = ReceiveTimestamp.Zero;
        TransmitTimestamp = TransmitTimestamp.Zero;
    }

    public static TransmitPacketHeader CreateNew()
    {
        return new ();
    }
}
