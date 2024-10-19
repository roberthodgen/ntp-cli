namespace RobertHodgen.Ntp.Client;

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
    public LeapIndicator LeapIndicator { get; protected init; }

    public VersionNumber VersionNumber { get; protected init; }

    public Mode Mode { get; protected init; }

    public Stratum Stratum { get; protected init; }

    public Poll Poll { get; protected init; }

    public Precision Precision { get; protected init; }

    public RootDelay RootDelay { get; protected init; }

    public RootDispersion RootDispersion { get; protected init; }

    public ReferenceId ReferenceId { get; protected init; }

    public ReferenceTimestamp ReferenceTimestamp { get; protected init; }

    public OriginTimestamp OriginTimestamp { get; protected init; }

    public ReceiveTimestamp ReceiveTimestamp { get; protected init; }

    public TransmitTimestamp TransmitTimestamp { get; protected init; }

    public bool KissODeath => Stratum == Stratum.UnspecifiedOrInvalid;

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

        var nth = 1;
        foreach (var b in words)
        {
            Console.Write($"{Convert.ToString(b, toBase: 2).PadLeft(8, '0'),8}");
            if (nth % 4 == 0)
            {
                Console.WriteLine();
            }
            else
            {
                Console.Write(" ");
            }

            nth++;
        }

        return words;
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
