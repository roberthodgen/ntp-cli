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

    public static RootDelay Parse(Memory<byte> memory) => new (NtpShort.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
