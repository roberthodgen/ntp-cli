namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// LI Leap Indicator (leap): 2-bit integer warning of an impending leap second to be inserted or deleted in the last
/// minute of the current month with values defined as:
/// +-------+----------------------------------------+
/// | Value | Meaning                                |
/// +-------+----------------------------------------+
/// | 0     | no warning                             |
/// | 1     | last minute of the day has 61 seconds  |
/// | 2     | last minute of the day has 59 seconds  |
/// | 3     | unknown (clock unsynchronized)         |
/// +-------+----------------------------------------+
/// </summary>
public sealed record LeapIndicator : EncodableBase
{
    public static LeapIndicator NoWarning => new (0);

    public static LeapIndicator LastMinuteOfTheDayHas61Seconds => new (1);

    public static LeapIndicator LastMinuteOfTheDayHas59Seconds => new (2);

    public static LeapIndicator Unknown => new (3);

    public byte Value { get; }

    public override int SizeInBits => 2;

    private LeapIndicator(byte value)
    {
        Value = value;
    }

    public override byte[] Encode() => [Value];

    public override string ToString() => Value switch
    {
        0 => "No warning",
        1 => "Last minute of the day has 61 seconds",
        2 => "Last minute of the day has 59 seconds",
        3 => "Unknown (clock unsynchronized)",
        _ => throw new ApplicationException("Out of range."),
    };
}
