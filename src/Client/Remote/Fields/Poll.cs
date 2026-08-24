namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// 8-bit signed integer representing the maximum interval between successive messages, in log2 seconds. Suggested
/// default limits for minimum and maximum poll intervals are 6 and 10, respectively.
/// </summary>
public sealed record Poll : EncodableBase
{
    public static Poll MinimumRecommended => new (6);

    public static Poll MaximumRecommended => new (10);

    /// <summary>
    /// Gets the poll interval value in log2 seconds.
    /// </summary>
    public sbyte Value { get; }

    public override int SizeInBits => 8;

    private Poll(sbyte value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="Poll"/> from a raw byte value received on the wire.
    /// </summary>
    /// <param name="value">The raw poll interval byte.</param>
    /// <returns>A new <see cref="Poll"/> instance.</returns>
    public static Poll Reconstitute(sbyte value) => new (value);

    public override byte[] Encode() => [(byte)Value];

    public override string ToString() => (Value) < 0 ? $"{1.0 / (1L << -(Value))}" : $"{1L << (Value)}";
}
