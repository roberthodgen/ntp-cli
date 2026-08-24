namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Transmit Timestamp (xmt): Time at the server when the response left for the client, in NTP timestamp format.
/// </summary>
public sealed record TransmitTimestamp : EncodableBase
{
    private const int TimestampSizeInBits = 64;

    private readonly IMonotonicClock? _clock;
    private NtpTimestamp? _value;

    public static TransmitTimestamp Zero => new (NtpTimestamp.Zero);

    /// <summary>
    /// Creates a transmit timestamp that captures the clock when encoded.
    /// </summary>
    /// <param name="clock">The monotonic clock to read from.</param>
    /// <returns>A new <see cref="TransmitTimestamp"/> instance with deferred capture.</returns>
    public static TransmitTimestamp FromClock(IMonotonicClock clock) => new (clock);

    /// <summary>
    /// Gets the captured NTP timestamp value.
    /// </summary>
    /// <exception cref="ApplicationException">Thrown when a deferred timestamp is read before it is encoded.</exception>
    public NtpTimestamp Value => _value ?? throw new ApplicationException("transmit timestamp accessed before being sent");

    public override int SizeInBits => TimestampSizeInBits;

    private TransmitTimestamp(NtpTimestamp value)
    {
        _value = value;
    }

    private TransmitTimestamp(IMonotonicClock clock)
    {
        _clock = clock;
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

    public override byte[] Encode()
    {
        _value ??= _clock?.Capture() ?? throw new ApplicationException("transmit timestamp cannot be encoded without a clock");
        return _value.Encode();
    }

    public override string ToString() => Value.ToString();
}
