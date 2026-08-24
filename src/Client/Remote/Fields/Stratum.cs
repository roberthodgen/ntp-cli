namespace RobertHodgen.Ntp.Client.Remote.Fields;

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

    /// <summary>
    /// Gets the stratum value.
    /// </summary>
    public byte Value { get; }

    public override int SizeInBits => 8;

    private Stratum(byte value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="Stratum"/> from a raw byte value received on the wire.
    /// </summary>
    /// <param name="stratum">The raw stratum byte.</param>
    /// <returns>A new <see cref="Stratum"/> instance.</returns>
    public static Stratum Reconstitute(byte stratum) => new (stratum);

    public override byte[] Encode() => [Value];

    public override string ToString() => Value switch
    {
        0 => "Unspecified or Invalid",
        1 => "Primary Server (e.g., equipped with a GPS receiver)",
        >= 2 and <= 15 => "Secondary Server (via NTP)",
        16 => "Unsynchronized",
        _ => "Reserved",
    };
}
