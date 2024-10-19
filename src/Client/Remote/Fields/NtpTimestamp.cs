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
    private const long UnixEpochSecondFromEra0 = 2208988800L;

    public static NtpTimestamp Zero => new (0, 0);

    /// <summary>
    /// Gets a NTP timestamp representing the current time.
    /// </summary>
    public static NtpTimestamp Now => FromDateTime(DateTime.UtcNow);

    public uint Seconds { get; }

    public uint Fraction { get; }

    public override int SizeInBits => 8 * 8;

    private NtpTimestamp(uint seconds, uint fraction)
    {
        Seconds = seconds;
        Fraction = fraction;
    }

    public static NtpTimestamp FromDateTime(DateTime time)
    {
        var diffFromEpoch = (time - DateTime.UnixEpoch);
        var seconds = Convert.ToUInt32(Math.Floor(diffFromEpoch.TotalSeconds));
        var fraction = Convert.ToUInt32((diffFromEpoch.TotalSeconds - seconds) * uint.MaxValue);
        return new (seconds, fraction);
    }

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

    public DateTime ToDateTime() => DateTime.UnixEpoch
        .AddSeconds(Seconds - UnixEpochSecondFromEra0 + (Fraction / (double)uint.MaxValue));

    public override string ToString() => ToDateTime().ToString("O");
}
