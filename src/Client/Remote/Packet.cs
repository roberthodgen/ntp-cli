namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// The message sent and received from NTP servers.
/// </summary>
public sealed record Packet
{
    public PacketHeaderBase Header { get; }

    public List<ExtensionField> Extensions { get; } = [];

    public KeyId KeyId { get; }

    public MessageDigest MessageDigest { get; }

    private Packet(PacketHeaderBase header)
    {
        Header = header;
        KeyId = KeyId.None;
        MessageDigest = MessageDigest.None;
    }

    public static Packet CreateNew(TransmitPacketHeader header)
    {
        return new (header);
    }

    public static Packet ParseResponse(Memory<byte> response)
    {
        if (response.Length < 48)
        {
            throw new ArgumentException("Response must be at least 48 bytes.");
        }

        return new (ReceivePacketHeader.Parse(response[..48]));
    }

    public byte[] Encode() => Header.Encode(); // TODO: handle KeyID and digest, if necessary
}
