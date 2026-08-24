namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// The message sent and received from NTP servers.
/// </summary>
public sealed record Packet<THeader>
    where THeader : PacketHeaderBase
{
    /// <summary>
    /// Gets the packet header containing all NTP protocol fields.
    /// </summary>
    public THeader Header { get; }

    /// <summary>
    /// Gets the list of extension fields attached to the packet.
    /// </summary>
    public List<ExtensionField> Extensions { get; } = [];

    /// <summary>
    /// Gets the key identifier for authentication.
    /// </summary>
    public KeyId KeyId { get; }

    /// <summary>
    /// Gets the message digest for authentication.
    /// </summary>
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

    /// <summary>
    /// Creates a new packet from the given header without a destination timestamp.
    /// </summary>
    /// <param name="header">The packet header.</param>
    /// <returns>A new <see cref="Packet{THeader}"/> instance.</returns>
    public static Packet<THeader> CreateNewFromHeader(THeader header)
    {
        return new (header);
    }

    /// <summary>
    /// Creates a new packet from the given header with a destination timestamp.
    /// </summary>
    /// <param name="header">The packet header.</param>
    /// <param name="destinationTimestamp">The time when the reply arrived from the server.</param>
    /// <returns>A new <see cref="Packet{THeader}"/> instance.</returns>
    public static Packet<THeader> CreateNewFromHeaderWithDestinationTimestamp(
        THeader header,
        NtpTimestamp destinationTimestamp)
    {
        return new (header, destinationTimestamp);
    }

    /// <summary>
    /// Encodes the packet header to its wire-format byte representation.
    /// </summary>
    /// <returns>The encoded packet bytes.</returns>
    /// <remarks>
    /// This client does not implement NTP authentication or extension field serialization.
    /// Requests are unauthenticated 48-byte headers; received extension bytes are accepted but not re-encoded.
    /// </remarks>
    public byte[] Encode() => Header.Encode();
}
