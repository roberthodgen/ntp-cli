namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Version Number (version): 3-bit integer representing the NTP version number, currently 4.
/// </summary>
public sealed record VersionNumber : EncodableBase
{
    /// <summary>
    /// Default value.
    /// </summary>
    public static VersionNumber Four => new (4);

    public byte Value { get; }

    public override int SizeInBits => 3;

    private VersionNumber(byte value)
    {
        Value = value;
    }

    public static VersionNumber Reconstitute(byte version) => new (version);

    public override byte[] Encode() => [Value];

    public override string ToString()
    {
        return $"v{Value}";
    }
}
