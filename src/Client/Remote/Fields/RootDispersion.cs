namespace RobertHodgen.Ntp.Client.Remote.Fields;

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

    public static RootDispersion Parse(Memory<byte> memory) => new (NtpShort.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
