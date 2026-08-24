namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Version Number (version): 3-bit integer representing the NTP version number, currently 4.
/// </summary>
public sealed record VersionNumber : EncodableBase
{
    /// <summary>
    /// Default value.
    /// </summary>
    public static VersionNumber Four => new (4);

    /// <summary>
    /// Gets the NTP version number value.
    /// </summary>
    public byte Value { get; }

    public override int SizeInBits => 3;

    private VersionNumber(byte value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="VersionNumber"/> from a raw byte value received on the wire.
    /// </summary>
    /// <param name="version">The raw version number byte.</param>
    /// <returns>A new <see cref="VersionNumber"/> instance.</returns>
    public static VersionNumber Reconstitute(byte version) => new (version);

    public override byte[] Encode() => [Value];

    public override string ToString() => $"v{Value}";
}
