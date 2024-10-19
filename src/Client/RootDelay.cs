namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Root Delay: Total round-trip delay to the reference clock, in NTP short format.
/// </summary>
public sealed record RootDelay : EncodableBase
{
    public static RootDelay Zero => new (NtpShort.Zero);

    public NtpShort Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private RootDelay(NtpShort value)
    {
        Value = value;
    }

    public override byte[] Encode() => Value.Encode();
}
