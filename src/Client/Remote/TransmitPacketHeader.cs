namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// Transmit Packet Header (<c>x.</c>).
/// </summary>
/// <remarks>
/// OriginTimestamp.Zero is intentional. The client request sends a zero origin
/// timestamp on the wire and writes client departure time to the request transmit
/// timestamp. The server echoes that request transmit timestamp back in the
/// response's origin field, which becomes T1 for Theta()/Delta() per RFC 5905
/// Section 8. See AGENTS.md for the Encode-time request xmt capture contract.
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

    /// <summary>
    /// Creates a new NTP request packet ready to be sent to an NTP server.
    /// </summary>
    /// <param name="clock">The monotonic clock to use for the transmit timestamp.</param>
    /// <returns>A new <see cref="Packet{TransmitPacketHeader}"/> instance.</returns>
    public static Packet<TransmitPacketHeader> CreateNewPacket(IMonotonicClock clock)
    {
        return Packet<TransmitPacketHeader>.CreateNewFromHeader(new TransmitPacketHeader(clock));
    }
}
