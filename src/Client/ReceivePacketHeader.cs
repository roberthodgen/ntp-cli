namespace RobertHodgen.Ntp.Client;

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

    public static ReceivePacketHeader Parse(Memory<byte> response)
    {
        if (response.Length != 48)
        {
            throw new ArgumentException("Header must be 48 bytes.", nameof(response));
        }

        var word0 = BitConverter.ToUInt32(response[..4].ToArray());
        // TODO parse word 0

        var rootDelay = RootDelay.Parse(response[4..8]);
        var rootDispersion = RootDispersion.Parse(response[8..12]);
        var referenceId = ReferenceId.Parse(response[12..16]);
        var referenceTimestamp = ReferenceTimestamp.Parse(response[16..24]);
        var originTimestamp = OriginTimestamp.Parse(response[24..32]);
        var receiveTimestamp = ReceiveTimestamp.Parse(response[32..40]);
        var transmitTimestamp = TransmitTimestamp.Parse(response[40..48]);

        return new (
            LeapIndicator.NoWarning, // TODO
            VersionNumber.Four, // TODO
            Mode.Server, // TODO
            Stratum.UnspecifiedOrInvalid, // TODO
            Poll.MaximumRecommended, // TODO
            Precision.Microsecond, // TODO
            rootDelay,
            rootDispersion,
            referenceId,
            referenceTimestamp,
            originTimestamp,
            receiveTimestamp,
            transmitTimestamp);
    }
}
