namespace Roberthodgen.Ntp.Client.Tests;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;
using System.Reflection;

public class SampleTests
{
    [Fact]
    public void Theta_WithSymmetricDelay_ReturnsZeroOffset()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0 = baseTime;
        var t1 = baseTime.AddMilliseconds(10);
        var t2 = baseTime.AddMilliseconds(20);
        var t3 = baseTime.AddMilliseconds(30);

        var sample = CreateSample(t0, t1, t2, t3);

        var theta = sample.Theta();

        theta.TotalMilliseconds.ShouldBe(0.0, 1);
    }

    [Fact]
    public void Theta_UsesResponseOriginTimestampAsClientTransmitTime()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0Ntp = NtpTimestamp.FromDateTime(baseTime);
        var t1Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(10));
        var t2Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(20));
        var t3Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(30));

        var clientPacket = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        var serverPacket = CreateServerPacket(t0Ntp, t1Ntp, t2Ntp, t3Ntp);
        var sample = CreateSampleWithPackets(clientPacket, serverPacket);

        var theta = sample.Theta();

        theta.TotalMilliseconds.ShouldBe(0.0, 1);
    }

    [Fact]
    public void Theta_WithPositiveOffset_ReturnsCorrectOffset()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0 = baseTime;
        var t1 = baseTime.AddMilliseconds(10);
        var t2 = baseTime.AddMilliseconds(20);
        var t3 = baseTime.AddMilliseconds(25);

        var sample = CreateSample(t0, t1, t2, t3);

        var theta = sample.Theta();

        theta.TotalMilliseconds.ShouldBe(2.5, 1);
    }

    [Fact]
    public void Theta_WithNegativeOffset_ReturnsCorrectOffset()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0 = baseTime;
        var t1 = baseTime.AddMilliseconds(10);
        var t2 = baseTime.AddMilliseconds(15);
        var t3 = baseTime.AddMilliseconds(30);

        var sample = CreateSample(t0, t1, t2, t3);

        var theta = sample.Theta();

        theta.TotalMilliseconds.ShouldBe(-2.5, 1);
    }

    [Fact]
    public void Delta_WithSymmetricDelay_ReturnsCorrectRoundTrip()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0 = baseTime;
        var t1 = baseTime.AddMilliseconds(10);
        var t2 = baseTime.AddMilliseconds(20);
        var t3 = baseTime.AddMilliseconds(30);

        var sample = CreateSample(t0, t1, t2, t3);

        var delta = sample.Delta();

        delta.TotalMilliseconds.ShouldBe(20.0, 1);
    }

    [Fact]
    public void Delta_UsesResponseOriginTimestampAsClientTransmitTime()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0Ntp = NtpTimestamp.FromDateTime(baseTime);
        var t1Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(10));
        var t2Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(20));
        var t3Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(30));

        var clientPacket = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        var serverPacket = CreateServerPacket(t0Ntp, t1Ntp, t2Ntp, t3Ntp);
        var sample = CreateSampleWithPackets(clientPacket, serverPacket);

        var delta = sample.Delta();

        delta.TotalMilliseconds.ShouldBe(20.0, 1);
    }

    [Fact]
    public void Delta_WithAsymmetricDelay_ReturnsCorrectRoundTrip()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0 = baseTime;
        var t1 = baseTime.AddMilliseconds(5);
        var t2 = baseTime.AddMilliseconds(10);
        var t3 = baseTime.AddMilliseconds(30);

        var sample = CreateSample(t0, t1, t2, t3);

        var delta = sample.Delta();

        delta.TotalMilliseconds.ShouldBe(25.0, 1);
    }

    [Fact]
    public void Delta_WithZeroDelay_ReturnsZero()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0 = baseTime;
        var t1 = baseTime;
        var t2 = baseTime;
        var t3 = baseTime;

        var sample = CreateSample(t0, t1, t2, t3);

        var delta = sample.Delta();

        delta.TotalMilliseconds.ShouldBe(0.0, 1);
    }

    [Fact]
    public void Theta_WithoutDestinationTimestamp_Throws()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0Ntp = NtpTimestamp.FromDateTime(baseTime);
        var t1Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(10));
        var t2Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(20));

        var clientPacket = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        var serverPacket = CreateServerPacket(t0Ntp, t1Ntp, t2Ntp, (NtpTimestamp?)null);

        var sample = CreateSampleWithPackets(clientPacket, serverPacket);

        Should.Throw<ApplicationException>(() => sample.Theta());
    }

    [Fact]
    public void Delta_WithoutDestinationTimestamp_Throws()
    {
        var baseTime = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var t0Ntp = NtpTimestamp.FromDateTime(baseTime);
        var t1Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(10));
        var t2Ntp = NtpTimestamp.FromDateTime(baseTime.AddMilliseconds(20));

        var clientPacket = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        var serverPacket = CreateServerPacket(t0Ntp, t1Ntp, t2Ntp, (NtpTimestamp?)null);

        var sample = CreateSampleWithPackets(clientPacket, serverPacket);

        Should.Throw<ApplicationException>(() => sample.Delta());
    }

    private static Sample CreateSample(
        DateTime t0, DateTime t1, DateTime t2, DateTime t3)
    {
        var t0Ntp = NtpTimestamp.FromDateTime(t0);
        var t1Ntp = NtpTimestamp.FromDateTime(t1);
        var t2Ntp = NtpTimestamp.FromDateTime(t2);
        var t3Ntp = NtpTimestamp.FromDateTime(t3);

        var clientPacket = TransmitPacketHeader.CreateNewPacket(new MonotonicClock());
        var originField = typeof(PacketHeaderBase).GetField(
            "<OriginTimestamp>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        originField.SetValue(clientPacket.Header, OriginTimestamp.Parse(t0Ntp.Encode()));

        var serverPacket = CreateServerPacket(t0Ntp, t1Ntp, t2Ntp, t3Ntp);

        return CreateSampleWithPackets(clientPacket, serverPacket);
    }

    private static Packet<ReceivePacketHeader> CreateServerPacket(
        NtpTimestamp t0Ntp, NtpTimestamp t1Ntp, NtpTimestamp t2Ntp, NtpTimestamp? t3Ntp)
    {
        var types = new Type[]
        {
            typeof(LeapIndicator), typeof(VersionNumber), typeof(Mode),
            typeof(Stratum), typeof(Poll), typeof(Precision),
            typeof(RootDelay), typeof(RootDispersion), typeof(ReferenceId),
            typeof(ReferenceTimestamp), typeof(OriginTimestamp),
            typeof(ReceiveTimestamp), typeof(TransmitTimestamp)
        };

        var args = new object[]
        {
            LeapIndicator.NoWarning,
            VersionNumber.Four,
            Mode.Server,
            Stratum.Primary,
            Poll.MinimumRecommended,
            Precision.Microsecond,
            RootDelay.Zero,
            RootDispersion.Zero,
            ReferenceId.Empty,
            ReferenceTimestamp.Zero,
            OriginTimestamp.Parse(t0Ntp.Encode()),
            ReceiveTimestamp.Parse(t1Ntp.Encode()),
            TransmitTimestamp.Parse(t2Ntp.Encode())
        };

        var ctor = typeof(ReceivePacketHeader).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            types,
            null);

        if (ctor == null)
        {
            throw new InvalidOperationException("ReceivePacketHeader constructor not found");
        }

        var header = (ReceivePacketHeader)ctor.Invoke(args)!;

        if (t3Ntp != null)
            return Packet<ReceivePacketHeader>.CreateNewFromHeaderWithDestinationTimestamp(header, t3Ntp);

        return Packet<ReceivePacketHeader>.CreateNewFromHeader(header);
    }

    private static Sample CreateSampleWithPackets(
        Packet<TransmitPacketHeader> clientPacket,
        Packet<ReceivePacketHeader> serverPacket)
    {
        var types = new Type[]
        {
            typeof(Packet<TransmitPacketHeader>),
            typeof(Packet<ReceivePacketHeader>)
        };

        var args = new object[] { clientPacket, serverPacket };

        var ctor = typeof(Sample).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            types,
            null);

        if (ctor == null)
        {
            throw new InvalidOperationException("Sample constructor not found");
        }

        return (Sample)ctor.Invoke(args)!;
    }
}
