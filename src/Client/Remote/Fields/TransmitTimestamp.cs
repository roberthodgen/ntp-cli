using RobertHodgen.Ntp.Client.Remote;

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

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
