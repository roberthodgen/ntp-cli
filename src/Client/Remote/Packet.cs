namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// The message sent and received from NTP servers.
/// </summary>
public sealed record Packet<THeader>
    where THeader : PacketHeaderBase
{
    public THeader Header { get; }

    public List<ExtensionField> Extensions { get; } = [];

    public KeyId KeyId { get; }

    public MessageDigest MessageDigest { get; }

    /// <summary>
    /// Time at the client when the reply arrived from the server.
    /// </summary>
    public NtpTimestamp? DestinationTimestamp { get; }

    private Packet(THeader header, NtpTimestamp? destinationTimestamp = null)
    {
        Header = header;
        KeyId = KeyId.None;
        MessageDigest = MessageDigest.None;
        DestinationTimestamp = destinationTimestamp;
    }

    public static Packet<THeader> CreateNewFromHeader(THeader header)
    {
        return new (header);
    }

    public static Packet<THeader> CreateNewFromHeaderWithDestinationTimestamp(
        THeader header,
        NtpTimestamp destinationTimestamp)
    {
        return new (header, destinationTimestamp);
    }

    public byte[] Encode() => Header.Encode(); // TODO: handle KeyID and digest, if necessary
}
