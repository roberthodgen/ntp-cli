using RobertHodgen.Ntp.Client.Remote;

namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Origin Timestamp (org): Time at the client when the request departed for the server, in NTP timestamp format.
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
    /// Creates an origin timestamp that defers capturing the clock time until encode.
    /// </summary>
    /// <param name="clock">The monotonic clock to capture at encode time.</param>
    /// <returns>A new <see cref="OriginTimestamp"/> instance that captures time at encode.</returns>
    public static OriginTimestamp SerializableFromClock(IMonotonicClock clock) => new (NtpTimestamp.Zero, () => clock.Capture());

    internal static OriginTimestamp CreateSerializableForTesting(Func<NtpTimestamp> timestampFactory) =>
        new (NtpTimestamp.Zero, timestampFactory);

    /// <summary>
    /// Gets the NTP timestamp value.
    /// </summary>
    public NtpTimestamp Value { get; }

    private readonly Func<NtpTimestamp>? _timestampFactory;

    public override int SizeInBits => Value.SizeInBits;

    private OriginTimestamp(NtpTimestamp value, Func<NtpTimestamp>? timestampFactory = null)
    {
        Value = value;
        _timestampFactory = timestampFactory;
    }

    /// <summary>
    /// Parses an NTP timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">8 bytes of NTP timestamp data in network byte order.</param>
    /// <returns>A new <see cref="OriginTimestamp"/> instance.</returns>
    public static OriginTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => (_timestampFactory?.Invoke() ?? Value).Encode();

    public override string ToString() => _timestampFactory is not null
        ? "(deferred — encoded at Encode() time)"
        : Value.ToString();
}
