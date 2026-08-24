namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Receive Timestamp (rec): Time at the server when the request arrived from the client, in NTP timestamp format.
/// </summary>
public sealed record ReceiveTimestamp : EncodableBase
{
    public static ReceiveTimestamp Zero => new (NtpTimestamp.Zero);

    /// <summary>
    /// Gets the NTP timestamp value.
    /// </summary>
    public NtpTimestamp Value { get; }

    public override int SizeInBits => Value.SizeInBits;

    private ReceiveTimestamp(NtpTimestamp value)
    {
        Value = value;
    }

    /// <summary>
    /// Parses an NTP timestamp from the given byte memory.
    /// </summary>
    /// <param name="memory">8 bytes of NTP timestamp data in network byte order.</param>
    /// <returns>A new <see cref="ReceiveTimestamp"/> instance.</returns>
    public static ReceiveTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => Value.Encode();

    public override string ToString() => Value.ToString();
}
