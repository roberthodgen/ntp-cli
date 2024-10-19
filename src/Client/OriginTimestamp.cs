namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Origin Timestamp (org): Time at the client when the request departed for the server, in NTP timestamp format.
/// </summary>
public sealed record OriginTimestamp : EncodableBase
{
    public static OriginTimestamp Now => new (NtpTimestamp.FromDateTime(DateTime.UtcNow));

    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private OriginTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    public static OriginTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
