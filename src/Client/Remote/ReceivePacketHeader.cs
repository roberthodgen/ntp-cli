namespace RobertHodgen.Ntp.Client.Remote;

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
}
