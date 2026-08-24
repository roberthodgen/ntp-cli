namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Root Delay: Total round-trip delay to the reference clock, in NTP short format.
/// </summary>
public sealed record RootDelay : EncodableBase
{
    public static RootDelay Zero => new (NtpShort.Zero);

    /// <summary>
    /// Gets the NTP short format value for total round-trip delay.
    /// </summary>
    public NtpShort Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private RootDelay(NtpShort value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses an NTP short timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">4 bytes of NTP short timestamp data in network byte order.</param>
    /// <returns>A new <see cref="RootDelay"/> instance.</returns>
    public static RootDelay Parse(Memory<byte> memory) => new (NtpShort.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
