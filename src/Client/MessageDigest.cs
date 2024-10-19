namespace RobertHodgen.Ntp.Client;

using System.Security.Cryptography;

/// <summary>
/// Message Digest (digest): 128-bit MD5 hash computed over the key followed by the NTP packet header and extensions
/// fields (but not the  Key Identifier or Message Digest fields).
/// </summary>
/// <remarks>
/// It should be noted that the MAC computation used here differs from those defined in [RFC1305] and [RFC4330] but is
/// consistent with how existing implementations generate a MAC.
/// </remarks>
public sealed record MessageDigest : EncodableBase
{
    public static MessageDigest None => new ([]);

    public byte[] Value { get; }

    public override int SizeInBits => Value.Length * 8;
    
    private MessageDigest(byte[] value)
    {
        if (value.Length != 16)
        {
            if (value.Length != 0)
            {
                throw new ArgumentException("The MD5 hash value is not 16-bytes long.", nameof(value));
            }
        }

        Value = value;
    }

    public static MessageDigest CreateNew(byte[] input) => new (MD5.HashData(input));

    public override byte[] Encode() => Value;
}
