namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Reference Timestamp: Time when the system clock was last set or corrected, in NTP timestamp format.
/// </summary>
public sealed record ReferenceTimestamp : EncodableBase
{
    public static ReferenceTimestamp Zero => new (NtpTimestamp.Zero);
    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private ReferenceTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    public override byte[] Encode() => Value.Encode();
}
