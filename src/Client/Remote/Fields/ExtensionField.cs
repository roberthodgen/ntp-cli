namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// NTP Extension Field as defined in RFC 5905 Section 7.4.
/// </summary>
public sealed record ExtensionField : EncodableBase
{
    /// <summary>
    /// Gets an empty extension field instance.
    /// </summary>
    public static ExtensionField None => new ([]);

    /// <summary>
    /// Gets the raw bytes of the extension field.
    /// </summary>
    public byte[] Value { get; }

    /// <summary>
    /// Gets the 16-bit field type identifier.
    /// </summary>
    public ushort FieldType { get; }

    /// <summary>
    /// Gets the length of the extension field data in 64-bit words.
    /// </summary>
    public ushort Length { get; }

    public override int SizeInBits => Value.Length * 8; // simplified; need to account for type and length

    private ExtensionField(byte[] value)
    {
        Value = value;
    }

    public override byte[] Encode() => throw new NotImplementedException();
}
