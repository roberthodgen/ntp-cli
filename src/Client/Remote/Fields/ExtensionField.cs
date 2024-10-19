namespace RobertHodgen.Ntp.Client.Remote.Fields;

public sealed record ExtensionField : EncodableBase
{
    public static ExtensionField None => new ([]);

    public byte[] Value { get; }

    public ushort FieldTye { get; }

    public ushort Length { get; }

    public override int SizeInBits => Value.Length * 8; // simplified; need to account for type and length

    private ExtensionField(byte[] value)
    {
        Value = value;
    }

    public override byte[] Encode() => throw new NotImplementedException();
}
