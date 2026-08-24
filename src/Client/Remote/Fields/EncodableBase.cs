namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Base type for all NTP packet fields that can be encoded to bytes.
/// </summary>
public abstract record EncodableBase
{
    /// <summary>
    /// The number of bits this field contains.
    /// </summary>
    public abstract int SizeInBits { get; }

    /// <summary>
    /// Gets a representation of the field in bytes (big endian order for numbers).
    /// </summary>
    /// <remarks>
    /// Network (big) endianness is used; e.g.:
    /// <code>
    /// if (BitConverter.IsLittleEndian)
    /// {
    ///     Array.Reverse(bytes);
    /// }
    /// </code>
    /// </remarks>
    /// <returns>A representation of this field in network (big) endian byte order.</returns>
    public abstract byte[] Encode();
}
