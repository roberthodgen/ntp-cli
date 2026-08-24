namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// Transmit Packet Header (<c>x.</c>).
/// </summary>
/// <remarks>
/// OriginTimestamp.Zero is intentional. The client request sends a zero origin
/// timestamp on the wire; the server fills in t0 (its reception time) and echoes
/// it back in the response's origin field. Theta()/Delta() read t0 from
/// ServerResponse.Header.OriginTimestamp — not from the client request — so the
/// server-echoed value is what matters. Using SerializableNow would encode a
/// client-side timestamp that drifts from the actual wire transmit time,
/// degrading accuracy. See AGENTS.md for details.
/// </remarks>
public sealed record TransmitPacketHeader : PacketHeaderBase
{
    private TransmitPacketHeader(IMonotonicClock clock)
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
            ReferenceTimestamp.Zero,
            OriginTimestamp.Zero,
            ReceiveTimestamp.Zero,
            TransmitTimestamp.FromClock(clock))
    {
    }

    public static Packet<TransmitPacketHeader> CreateNewPacket(IMonotonicClock clock)
    {
        return Packet<TransmitPacketHeader>.CreateNewFromHeader(new TransmitPacketHeader(clock));
    }
}
