namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Reference Timestamp: Time when the system clock was last set or corrected, in NTP timestamp format.
/// </summary>
public sealed record ReferenceTimestamp : EncodableBase
{
    public static ReferenceTimestamp Zero => new (NtpTimestamp.Zero);
    /// <summary>
    /// Gets the NTP timestamp value for the reference time.
    /// </summary>
    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private ReferenceTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses an NTP timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">8 bytes of NTP timestamp data in network byte order.</param>
    /// <returns>A new <see cref="ReferenceTimestamp"/> instance.</returns>
    public static ReferenceTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
