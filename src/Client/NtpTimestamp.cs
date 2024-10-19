namespace RobertHodgen.Ntp.Client;

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
        var seconds = Convert.ToUInt32(diffFromEpoch.TotalSeconds);
        var fraction = Convert.ToUInt32((diffFromEpoch.TotalSeconds - seconds) * uint.MaxValue);
        return new (seconds, fraction);
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
}
