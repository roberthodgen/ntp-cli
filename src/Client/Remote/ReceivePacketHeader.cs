namespace RobertHodgen.Ntp.Client.Remote;

using RobertHodgen.Ntp.Client;
using Fields;

/// <summary>
/// Receive packet header (<c>r.</c>).
/// </summary>
public sealed record ReceivePacketHeader : PacketHeaderBase
{
    private ReceivePacketHeader(
        LeapIndicator leapIndicator,
        VersionNumber versionNumber,
        Mode mode,
        Stratum stratum,
        Poll poll,
        Precision precision,
        RootDelay rootDelay,
        RootDispersion rootDispersion,
        ReferenceId referenceId,
        ReferenceTimestamp referenceTimestamp,
        OriginTimestamp originTimestamp,
        ReceiveTimestamp receiveTimestamp,
        TransmitTimestamp transmitTimestamp)
        : base(
            leapIndicator,
            versionNumber,
            mode,
            stratum,
            poll,
            precision,
            rootDelay,
            rootDispersion,
            referenceId,
            referenceTimestamp,
            originTimestamp,
            receiveTimestamp,
            transmitTimestamp)
    {
    }

    /// <summary>
    /// Parses a server response packet from the given byte memory.
    /// </summary>
    /// <param name="response">The raw response bytes (at least 48 bytes).</param>
    /// <param name="destinationTimestamp">The local time when the response was received.</param>
    /// <returns>A new <see cref="Packet{ReceivePacketHeader}"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the response is less than 48 bytes.</exception>
    public static Packet<ReceivePacketHeader> Parse(Memory<byte> response, NtpTimestamp destinationTimestamp)
    {
        if (response.Length < 48)
        {
            throw new ArgumentException("Header must be at least 48 bytes.", nameof(response));
        }

        var word0Bytes = response[..4].ToArray();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(word0Bytes);
        }

        var word0 = BitConverter.ToUInt32(word0Bytes);
        var leapIndicator = LeapIndicator.Reconstitute((byte)((word0 >> 30) & 0b_11));
        var versionNumber = VersionNumber.Reconstitute((byte)((word0 >> 27) & 0b_111));
        var mode = Mode.Reconstitute((byte)((word0 >> 24) & 0b_111));
        var stratum = Stratum.Reconstitute(response.Span[1]);
        var poll = Poll.Reconstitute(unchecked((sbyte)response.Span[2]));
        var precision = Precision.Reconstitute(unchecked((sbyte)response.Span[3]));

        var rootDelay = RootDelay.Parse(response[4..8]);
        var rootDispersion = RootDispersion.Parse(response[8..12]);
        var referenceId = ReferenceId.Parse(response[12..16]);
        var referenceTimestamp = ReferenceTimestamp.Parse(response[16..24]);
        var originTimestamp = OriginTimestamp.Parse(response[24..32]);
        var receiveTimestamp = ReceiveTimestamp.Parse(response[32..40]);
        var transmitTimestamp = TransmitTimestamp.Parse(response[40..48]);

        return Packet<ReceivePacketHeader>.CreateNewFromHeaderWithDestinationTimestamp(
            new ReceivePacketHeader(
                leapIndicator,
                versionNumber,
                mode,
                stratum,
                poll,
                precision,
                rootDelay,
                rootDispersion,
                referenceId,
                referenceTimestamp,
                originTimestamp,
                receiveTimestamp,
                transmitTimestamp),
            destinationTimestamp);
    }

    /// <summary>
    /// Gets the kiss code if this is a Kiss-o'-Death packet (stratum 0), otherwise null.
    /// </summary>
    public KissCodes? KissCode => Stratum == Stratum.UnspecifiedOrInvalid
        ? KissCodes.CreateNew(ReferenceId.Value)
        : null;

    /// <summary>
    /// Validates the response and throws if the server sent a Kiss-o'-Death packet requiring client action.
    /// </summary>
    /// <exception cref="NtpKissODeathException">Thrown when the kiss code requires client action (DENY, RSTR, or RATE).</exception>
    public void ValidateKissODeath()
    {
        if (KissCode is { RequiresClientAction: true } kissCode)
        {
            throw new NtpKissODeathException(kissCode);
        }
    }

    /// <summary>
    /// Validates that a non-broadcast response echoes the request transmit timestamp in the origin timestamp field.
    /// </summary>
    /// <param name="requestTransmitTimestamp">The NtpTimestamp transmit timestamp sent in the client request.</param>
    /// <exception cref="ApplicationException">Thrown when the response origin timestamp is zero or mismatched.</exception>
    public void ValidateOriginTimestamp(TransmitTimestamp requestTransmitTimestamp)
    {
        if (Mode == Mode.Broadcast)
        {
            return;
        }

        if (OriginTimestamp.Value == NtpTimestamp.Zero)
        {
            throw new ApplicationException("Response origin timestamp is zero.");
        }

        if (OriginTimestamp.Value != requestTransmitTimestamp.Value)
        {
            throw new ApplicationException("Response origin timestamp does not match request transmit timestamp.");
        }
    }
}
