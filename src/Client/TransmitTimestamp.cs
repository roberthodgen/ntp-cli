namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Transmit Timestamp (xmt): Time at the server when the response left for the client, in NTP timestamp format.
/// </summary>
public sealed record TransmitTimestamp : EncodableBase
{
    public static TransmitTimestamp Zero => new (NtpTimestamp.Zero);

    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private TransmitTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    public override byte[] Encode() => Value.Encode();
}
