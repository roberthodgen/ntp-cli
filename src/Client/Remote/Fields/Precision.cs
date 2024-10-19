namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// 8-bit signed integer representing the precision of the system clock, in log2 seconds.  For instance, a value of -18
/// corresponds to a precision of about one microsecond.  The precision can be determined when the service first starts
/// up as the minimum time of several iterations to read the system clock.
/// </summary>
public sealed record Precision : EncodableBase
{
    public static Precision Microsecond => new (-18);

    public sbyte Value { get; }

    public override int SizeInBits => 8;

    private Precision(sbyte value)
    {
        Value = value;
    }

    public override byte[] Encode() => [(byte)Value];

    public override string ToString() => (Value) < 0 ? $"{1.0 / (1L << -(Value)):e2}" : $"{1L << (Value):e2}";
}
