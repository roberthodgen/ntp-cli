namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Key Identifier (keyid): 32-bit unsigned integer used by the client and server to designate a secret 128-bit MD5 key.
/// </summary>
public sealed record KeyId : EncodableBase
{
    public static KeyId None => new (0);

    public uint Value { get; }

    public override int SizeInBits => 32;

    private KeyId(uint value)
    {
        Value = value;
    }

    public override byte[] Encode()
    {
        var bytes = BitConverter.GetBytes(Value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }
}
