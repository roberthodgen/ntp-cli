namespace RobertHodgen.Ntp.Client.Remote.Fields;

/// <summary>
/// Origin Timestamp (org): Time at the client when the request departed for the server, in NTP timestamp format.
/// </summary>
public sealed record OriginTimestamp : EncodableBase
{
    public static OriginTimestamp Now => new (NtpTimestamp.Now);

    public static OriginTimestamp SerializableNow => new (NtpTimestamp.Zero, () => NtpTimestamp.Now);

    internal static OriginTimestamp CreateSerializableForTesting(Func<NtpTimestamp> timestampFactory) =>
        new (NtpTimestamp.Zero, timestampFactory);

    public NtpTimestamp Value { get; }

    private readonly Func<NtpTimestamp>? _timestampFactory;

    public override int SizeInBits => Value.SizeInBits;

    private OriginTimestamp(NtpTimestamp value, Func<NtpTimestamp>? timestampFactory = null)
    {
        Value = value;
        _timestampFactory = timestampFactory;
    }

    public static OriginTimestamp Parse(Memory<byte> memory) => new (NtpTimestamp.Parse(memory));

    public override byte[] Encode() => (_timestampFactory?.Invoke() ?? Value).Encode();

    public override string ToString() => _timestampFactory is not null
        ? "(deferred — encoded at Encode() time)"
        : Value.ToString();
}
