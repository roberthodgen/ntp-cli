namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Root Dispersion: Total dispersion to the reference clock, in NTP short format.
/// </summary>
public sealed record RootDispersion : EncodableBase
{
    public static RootDispersion Zero => new (NtpShort.Zero);

    public NtpShort Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private RootDispersion(NtpShort value)
    {
        Value = value;
    }

    public override byte[] Encode() => Value.Encode();
}
