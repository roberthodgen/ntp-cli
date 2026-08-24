namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Transmit Timestamp (xmt): Time at the server when the response left for the client, in NTP timestamp format.
/// </summary>
public sealed record TransmitTimestamp : EncodableBase
{
    public static TransmitTimestamp Zero => new (NtpTimestamp.Zero);

    /// <summary>
    /// Creates a transmit timestamp from the current clock time.
    /// </summary>
    /// <param name="clock">The monotonic clock to read from.</param>
    /// <returns>A new <see cref="TransmitTimestamp"/> instance.</returns>
    public static TransmitTimestamp FromClock(IMonotonicClock clock) => new (NtpTimestamp.FromClock(clock));

    /// <summary>
    /// Gets the NTP timestamp value.
    /// </summary>
    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private TransmitTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses an NTP timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">8 bytes of NTP timestamp data in network byte order.</param>
    /// <returns>A new <see cref="TransmitTimestamp"/> instance.</returns>
    public static TransmitTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    /// <summary>
    /// Factory method to reconstitute a <see cref="TransmitTimestamp"/> from an <see cref="NtpTimestamp"/>.
    /// </summary>
    public static TransmitTimestamp Reconstitute(NtpTimestamp timestamp)
    {
        return new (timestamp);
    }

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
