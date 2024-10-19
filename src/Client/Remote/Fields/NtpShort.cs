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

    public static NtpShort Zero => new (0, 0);

    public ushort Seconds { get; }

    public ushort Fraction { get; }

    public override int SizeInBits => 4 * 8;

    private NtpShort(ushort seconds, ushort fraction)
    {
        Seconds = seconds;
        Fraction = fraction;
    }

    public static NtpShort FromTimeSpan(TimeSpan timeSpan)
    {
        var seconds = Convert.ToUInt16(Math.Floor(timeSpan.TotalSeconds));
        var fraction = Convert.ToUInt16((timeSpan.TotalSeconds - seconds) * ushort.MaxValue);
        return new (seconds, fraction);
    }

    public static NtpShort Parse(Memory<byte> memory)
    {
        if (memory.Length != 4)
        {
            throw new ArgumentException("NTP short format must be 4 bytes long.", nameof(memory));
        }

        var seconds = new Span<byte>(memory[..2].ToArray());
        if (BitConverter.IsLittleEndian)
        {
            seconds.Reverse();
        }

        var precision = new Span<byte>(memory[2..4].ToArray());
        if (BitConverter.IsLittleEndian)
        {
            precision.Reverse();
        }

        return new (BitConverter.ToUInt16(seconds), BitConverter.ToUInt16(precision));
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

    public TimeSpan ToTimeSpan()
    {
        return new TimeSpan((Seconds * TicksPerSecond) + (Fraction / ushort.MaxValue * TicksPerSecond));
    }

    public override string ToString() => ToTimeSpan().ToString("G");
}
