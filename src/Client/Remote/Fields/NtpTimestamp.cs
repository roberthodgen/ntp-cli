using RobertHodgen.Ntp.Client.Remote;

namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// NTP Timestamp Format
/// <code>
/// 0                   1                   2                   3
/// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                            Seconds                            |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |                            Fraction                           |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// </code>
/// </summary>
public sealed record NtpTimestamp : EncodableBase
{
    private const uint UnixEpochSecondFromEra0 = 2208988800;

    private const double FractionDivisor = 4294967296d;

    public static NtpTimestamp Zero => new (0, 0);

    /// <summary>
    /// Creates an NTP timestamp from raw seconds and fraction values.
    /// </summary>
    /// <param name="seconds">Seconds since NTP era 0 (January 1, 1900).</param>
    /// <param name="fraction">Fractional seconds as a 32-bit fixed-point number.</param>
    /// <returns>A new <see cref="NtpTimestamp"/> instance.</returns>
    public static NtpTimestamp Create(uint seconds, uint fraction) => new (seconds, fraction);

    /// <summary>
    /// Creates an NTP timestamp from the current clock time.
    /// </summary>
    /// <param name="clock">The monotonic clock to read from.</param>
    /// <returns>A new <see cref="NtpTimestamp"/> instance.</returns>
    public static NtpTimestamp FromClock(IMonotonicClock clock) => clock.Capture();

    /// <summary>
    /// Gets the integer seconds portion of the timestamp (seconds since NTP era 0: January 1, 1900).
    /// </summary>
    public uint Seconds { get; }

    /// <summary>
    /// Gets the fractional seconds portion of the timestamp (32-bit).
    /// </summary>
    public uint Fraction { get; }

    public override int SizeInBits => 8 * 8;

    private NtpTimestamp(uint seconds, uint fraction)
    {
        Seconds = seconds;
        Fraction = fraction;
    }

    /// <summary>
    /// Creates an NTP timestamp from a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="time">The date and time to convert.</param>
    /// <returns>A new <see cref="NtpTimestamp"/> instance.</returns>
    public static NtpTimestamp FromDateTime(DateTime time)
    {
        var diffFromEpoch = (time - DateTime.UnixEpoch);
        var seconds = Convert.ToUInt32(Math.Floor(diffFromEpoch.TotalSeconds));
        var fraction = Convert.ToUInt32((diffFromEpoch.TotalSeconds - seconds) * FractionDivisor);
        return new (seconds + UnixEpochSecondFromEra0, fraction);
    }

    /// <summary>
    /// Parses an NTP timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">8 bytes of NTP timestamp data in network byte order.</param>
    /// <returns>A new <see cref="NtpTimestamp"/> instance.</returns>
    public static NtpTimestamp Parse(Memory<byte> memory)
    {
        if (memory.Length != 8)
        {
            throw new ArgumentException("NTP Timestamp format must be 8 bytes long.", nameof(memory));
        }

        var seconds = new Span<byte>(memory[..4].ToArray());
        if (BitConverter.IsLittleEndian)
        {
            seconds.Reverse();
        }

        var precision = new Span<byte>(memory[4..8].ToArray());
        if (BitConverter.IsLittleEndian)
        {
            precision.Reverse();
        }

        return new (BitConverter.ToUInt32(seconds), BitConverter.ToUInt32(precision));
    }

    public override byte[] Encode()
    {
        var seconds = BitConverter.GetBytes(Seconds);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(seconds);
        }

        var fraction = BitConverter.GetBytes(Fraction);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(fraction);
        }

        return [..seconds, ..fraction];
    }

    /// <summary>
    /// Converts this NTP timestamp to a <see cref="DateTime"/> value.
    /// </summary>
    /// <returns>The equivalent date and time.</returns>
    public DateTime ToDateTime() => DateTime.UnixEpoch
        .AddSeconds(Seconds - UnixEpochSecondFromEra0 + (Fraction / FractionDivisor));

    public override string ToString() => ToDateTime().ToString("O");
}
