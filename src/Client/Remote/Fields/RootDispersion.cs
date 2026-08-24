namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Root Dispersion: Total dispersion to the reference clock, in NTP short format.
/// </summary>
public sealed record RootDispersion : EncodableBase
{
    public static RootDispersion Zero => new (NtpShort.Zero);

    /// <summary>
    /// Gets the NTP short format value for total dispersion.
    /// </summary>
    public NtpShort Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private RootDispersion(NtpShort value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses an NTP short timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">4 bytes of NTP short timestamp data in network byte order.</param>
    /// <returns>A new <see cref="RootDispersion"/> instance.</returns>
    public static RootDispersion Parse(Memory<byte> memory) => new (NtpShort.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
