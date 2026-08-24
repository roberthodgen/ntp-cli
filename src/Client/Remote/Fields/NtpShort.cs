namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// NTP Short Format
/// <code>
/// 0                   1                   2                   3
/// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// |          Seconds              |           Fraction            |
/// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
/// </code>
/// </summary>
public sealed record NtpShort : EncodableBase
{
    private const long TicksPerSecond = TimeSpan.TicksPerSecond;

    private const double FractionDivisor = 65536d;

    public static NtpShort Zero => new (0, 0);

    /// <summary>
    /// Gets the integer seconds portion of the timestamp.
    /// </summary>
    public ushort Seconds { get; }

    /// <summary>
    /// Gets the fractional seconds portion of the timestamp (16-bit).
    /// </summary>
    public ushort Fraction { get; }

    public override int SizeInBits => 4 * 8;

    private NtpShort(ushort seconds, ushort fraction)
    {
        Seconds = seconds;
        Fraction = fraction;
    }

    /// <summary>
    /// Creates an NTP short timestamp from a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="timeSpan">The time span to convert.</param>
    /// <returns>A new <see cref="NtpShort"/> instance.</returns>
    public static NtpShort FromTimeSpan(TimeSpan timeSpan)
    {
        var seconds = Convert.ToUInt16(timeSpan.Ticks / TicksPerSecond);
        var fractionTicks = timeSpan.Ticks % TicksPerSecond;
        var fraction = Convert.ToUInt16((fractionTicks * 65536L) / TicksPerSecond);
        return new (seconds, fraction);
    }

    /// <summary>
    /// Parses an NTP short timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">4 bytes of NTP short timestamp data in network byte order.</param>
    /// <returns>A new <see cref="NtpShort"/> instance.</returns>
    public static NtpShort Parse(Memory<byte> memory)
    {
        if (memory.Length != 4)
        {
            throw new ArgumentException("NTP short format must be 4 bytes long.", nameof(memory));
        }

        var seconds = memory[..2].Span;
        if (BitConverter.IsLittleEndian)
        {
            seconds.Reverse();
        }

        var fraction = memory[2..4].Span;
        if (BitConverter.IsLittleEndian)
        {
            fraction.Reverse();
        }

        return new (BitConverter.ToUInt16(seconds), BitConverter.ToUInt16(fraction));
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
    /// Converts this NTP short timestamp to a <see cref="TimeSpan"/>.
    /// </summary>
    /// <returns>The equivalent time span.</returns>
    public TimeSpan ToTimeSpan()
    {
        return TimeSpan.FromSeconds(Seconds + (Fraction / FractionDivisor));
    }

    public override string ToString() => ToTimeSpan().ToString("G");
}
