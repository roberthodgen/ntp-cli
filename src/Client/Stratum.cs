namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Stratum (stratum): 8-bit integer representing the stratum, with values defined:
/// <code>
/// +--------+-----------------------------------------------------+
/// | Value  | Meaning                                             |
/// +--------+-----------------------------------------------------+
/// | 0      | unspecified or invalid                              |
/// | 1      | primary server (e.g., equipped with a GPS receiver) |
/// | 2-15   | secondary server (via NTP)                          |
/// | 16     | unsynchronized                                      |
/// | 17-255 | reserved                                            |
/// +--------+-----------------------------------------------------+
/// </code>
/// </summary>
public sealed record Stratum : EncodableBase
{
    public static Stratum UnspecifiedOrInvalid => new (0);

    public static Stratum Primary => new (1);

    public static Stratum Unsynchronized => new (16);

    public byte Value { get; }

    public override int SizeInBits => 8;

    private Stratum(byte value)
    {
        Value = value;
    }

    public static Stratum Reconstitute(byte stratum) => new (stratum);

    public override byte[] Encode() => [Value];

    public override string ToString() => Value switch
    {
        0 => "unspecified or invalid",
        1 => "primary server (e.g., equipped with a GPS receiver)",
        > 2 and < 16 => "secondary server (via NTP)",
        16 => "unsynchronized",
        _ => "reserved",
    };
}
