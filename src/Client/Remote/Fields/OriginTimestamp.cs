namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Origin Timestamp (org): Time at the client when the request left for the server, in NTP timestamp format.
/// </summary>
public sealed record OriginTimestamp : EncodableBase
{
    public static OriginTimestamp Zero => new (NtpTimestamp.Zero);

    /// <summary>
    /// Creates an origin timestamp from the current clock time.
    /// </summary>
    /// <param name="clock">The monotonic clock to read from.</param>
    /// <returns>A new <see cref="OriginTimestamp"/> instance.</returns>
    public static OriginTimestamp FromClock(IMonotonicClock clock) => new (NtpTimestamp.FromClock(clock));

    /// <summary>
    /// Gets the NTP timestamp value.
    /// </summary>
    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private OriginTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses an NTP timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">8 bytes of NTP timestamp data in network byte order.</param>
    /// <returns>A new <see cref="OriginTimestamp"/> instance.</returns>
    public static OriginTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
