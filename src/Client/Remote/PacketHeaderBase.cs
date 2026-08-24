namespace RobertHodgen.Ntp.Client.Remote;

using Fields;
using Serilog;

/// <summary>
/// Packet Header Format
/// <code>
/// 0                   1                   2                   3
/// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |LI | VN  |Mode |    Stratum     |     Poll      |  Precision   |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                         Root Delay                            |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                         Root Dispersion                       |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                          Reference ID                         |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// +                     Reference Timestamp (64)                  +
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// +                      Origin Timestamp (64)                    +
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// +                      Receive Timestamp (64)                   +
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// +                      Transmit Timestamp (64)                  +
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// .                                                               .
/// .                    Extension Field 1 (variable)               .
/// .                                                               .
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// .                                                               .
/// .                    Extension Field 2 (variable)               .
/// .                                                               .
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                          Key Identifier                       |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                                                               |
/// |                            dgst (128)                         |
/// |                                                               |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// </code>
/// </summary>
public abstract record PacketHeaderBase
{
    /// <summary>
    /// Gets the leap second indicator.
    /// </summary>
    public LeapIndicator LeapIndicator { get; }

    /// <summary>
    /// Gets the NTP version number.
    /// </summary>
    public VersionNumber VersionNumber { get; }

    /// <summary>
    /// Gets the protocol mode.
    /// </summary>
    public Mode Mode { get; }

    /// <summary>
    /// Gets the stratum level of the server.
    /// </summary>
    public Stratum Stratum { get; }

    /// <summary>
    /// Gets the maximum poll interval.
    /// </summary>
    public Poll Poll { get; }

    /// <summary>
    /// Gets the clock precision.
    /// </summary>
    public Precision Precision { get; }

    /// <summary>
    /// Gets the total round-trip delay to the reference clock.
    /// </summary>
    public RootDelay RootDelay { get; }

    /// <summary>
    /// Gets the total dispersion to the reference clock.
    /// </summary>
    public RootDispersion RootDispersion { get; }

    /// <summary>
    /// Gets the reference identifier of the server or reference clock.
    /// </summary>
    public ReferenceId ReferenceId { get; }

    /// <summary>
    /// Time when the system clock was last set or corrected, in NTP timestamp format.
    /// </summary>
    public ReferenceTimestamp ReferenceTimestamp { get; }

    /// <summary>
    /// Time at the client when the request departed for the server, in NTP timestamp format
    /// </summary>
    public OriginTimestamp OriginTimestamp { get; }

    /// <summary>
    /// Time at the server when the request arrived from the client, in NTP timestamp format.
    /// </summary>
    public ReceiveTimestamp ReceiveTimestamp { get; }

    /// <summary>
    /// Time at the server when the response left for the client, in NTP timestamp format.
    /// </summary>
    public TransmitTimestamp TransmitTimestamp { get; }

    /// <summary>
    /// Gets a value indicating whether this is a Kiss-o'-Death packet (stratum 0).
    /// </summary>
    public bool KissODeath => Stratum == Stratum.UnspecifiedOrInvalid;

    protected PacketHeaderBase(
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
    {
        LeapIndicator = leapIndicator;
        VersionNumber = versionNumber;
        Mode = mode;
        Stratum = stratum;
        Poll = poll;
        Precision = precision;
        RootDelay = rootDelay;
        RootDispersion = rootDispersion;
        ReferenceId = referenceId;
        ReferenceTimestamp = referenceTimestamp;
        OriginTimestamp = originTimestamp;
        ReceiveTimestamp = receiveTimestamp;
        TransmitTimestamp = transmitTimestamp;
    }

    /// <summary>
    /// Encodes the packet header to its wire-format byte representation.
    /// </summary>
    /// <returns>The encoded header bytes in network byte order.</returns>
    public byte[] Encode()
    {
        const int wordLengthInBits = 32; // 32-bit words

        //              0                   1                   2                   3
        //              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        //             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //             |LI | VN  |Mode |    Stratum     |     Poll      |  Precision   |
        //             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        uint word0 = 0b_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0_0;
        int word0Start = 0;

        // 12 4-byte words
        var words = new byte[12 * 4];
        var position = 0;
        foreach (var field in FieldOrder())
        {
            if (field.SizeInBits < wordLengthInBits)
            {
                var wordSized = Convert.ToUInt32(field.Encode()[0]);
                word0Start += field.SizeInBits;
                word0 |= wordSized << (wordLengthInBits - word0Start);
                continue;
            }

            if (position == 0)
            {
                var bytes = BitConverter.GetBytes(word0);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }

                bytes.CopyTo(words, position);
                position += wordLengthInBits / 8;
            }

            field.Encode().CopyTo(words, position);
            position += field.SizeInBits / 8;
        }

        return words;
    }

    /// <summary>
    /// Logs all header field values at debug level using Serilog.
    /// </summary>
    public void LogDebugData()
    {
        Log.Debug("Leap indicator: {LeapIndicator}", LeapIndicator);
        Log.Debug("Version number: {VersionNumber}", VersionNumber);
        Log.Debug("Mode: {Mode}", Mode);
        Log.Debug("Stratum: {Stratum}", Stratum);
        Log.Debug("Poll: {Poll}", Poll);
        Log.Debug("Precision: {Precision}", Precision);
        Log.Debug("Root delay: {RootDelay}", RootDelay);
        Log.Debug("Root dispersion: {RootDispersion}", RootDispersion);
        Log.Debug("Reference ID: {ReferenceId}", ReferenceId);
        Log.Debug("Reference timestamp: {ReferenceTimestamp}", ReferenceTimestamp);
        Log.Debug("Origin timestamp: {OriginTimestamp}", OriginTimestamp);
        Log.Debug("Receive timestamp: {ReceiveTimestamp}", ReceiveTimestamp);
        Log.Debug("Transmit timestamp: {TransmitTimestamp}", TransmitTimestamp);
    }

    private IEnumerable<EncodableBase> FieldOrder()
    {
        yield return LeapIndicator;
        yield return VersionNumber;
        yield return Mode;
        yield return Stratum;
        yield return Poll;
        yield return Precision; // end of 1st word
        yield return RootDelay;
        yield return RootDispersion;
        yield return ReferenceId;
        yield return ReferenceTimestamp;
        yield return OriginTimestamp;
        yield return ReceiveTimestamp;
        yield return TransmitTimestamp;
    }
}
