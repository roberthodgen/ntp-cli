namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Mode (mode): 3-bit integer representing the mode, with values defined as:
/// <code>
/// +-------+--------------------------+
/// | Value | Meaning                  |
/// +-------+--------------------------+
/// | 0     | reserved                 |
/// | 1     | symmetric active         |
/// | 2     | symmetric passive        |
/// | 3     | client                   |
/// | 4     | server                   |
/// | 5     | broadcast                |
/// | 6     | NTP control message      |
/// | 7     | reserved for private use |
/// +-------+--------------------------+
/// </code>
/// </summary>
public sealed record Mode : EncodableBase
{
    public static Mode Reserved => new (0);

    public static Mode SymmetricActive => new (1);

    public static Mode SymmetricPassive => new (2);

    public static Mode Client => new (3);

    public static Mode Server => new (4);

    public static Mode Broadcast => new (5);

    public static Mode NtpControlMessage => new (6);

    public static Mode ReservedForPrivateUse => new (7);

    public byte Value { get; }

    public override int SizeInBits => 3;

    private Mode(byte value)
    {
        Value = value;
    }

    public override byte[] Encode() => [Value];
}
