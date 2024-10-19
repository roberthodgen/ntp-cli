namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Receive Timestamp (rec): Time at the server when the request arrived from the client, in NTP timestamp format.
/// </summary>
public sealed record ReceiveTimestamp : EncodableBase
{
    public static ReceiveTimestamp Zero => new (NtpTimestamp.Zero);

    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private ReceiveTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    public static ReceiveTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
