namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReceivePacketHeaderTests
{
    [Fact]
    public void Parse_WithWord0Values_ParsesAllFirstWordFields()
    {
        var response = CreateResponse();
        response[0] = 0b_11_100_101;
        response[1] = 2;
        response[2] = 6;
        response[3] = unchecked((byte)-20);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.LeapIndicator.Value.ShouldBe((byte)3);
        packet.Header.VersionNumber.Value.ShouldBe((byte)4);
        packet.Header.Mode.Value.ShouldBe((byte)5);
        packet.Header.Stratum.Value.ShouldBe((byte)2);
        packet.Header.Poll.Value.ShouldBe((sbyte)6);
        packet.Header.Precision.Value.ShouldBe((sbyte)-20);
    }

    [Fact]
    public void Parse_WithBytesAfterHeader_AcceptsPacket()
    {
        var response = new byte[52];
        response[0] = 0b_00_100_100;
        response[1] = 1;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.VersionNumber.Value.ShouldBe((byte)4);
        packet.Header.Mode.Value.ShouldBe((byte)4);
    }

    [Fact]
    public void Parse_WithPacketShorterThanHeader_Throws()
    {
        Should.Throw<ArgumentException>(() => ReceivePacketHeader.Parse(new byte[47], NtpTimestamp.Zero))
            .ParamName.ShouldBe("response");
    }

    [Fact]
    public void Parse_WithFullHeader_ParsesAllTimestampFields()
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        response[1] = 1;

        var refTime = NtpTimestamp.FromDateTime(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var refBytes = refTime.Encode();
        refBytes.CopyTo(response, 16);

        var orgTime = NtpTimestamp.FromDateTime(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        var orgBytes = orgTime.Encode();
        orgBytes.CopyTo(response, 24);

        var recTime = NtpTimestamp.FromDateTime(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc));
        var recBytes = recTime.Encode();
        recBytes.CopyTo(response, 32);

        var xmtTime = NtpTimestamp.FromDateTime(new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc));
        var xmtBytes = xmtTime.Encode();
        xmtBytes.CopyTo(response, 40);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.ReferenceTimestamp.Value.Seconds.ShouldBe(refTime.Seconds);
        packet.Header.OriginTimestamp.Value.Seconds.ShouldBe(orgTime.Seconds);
        packet.Header.ReceiveTimestamp.Value.Seconds.ShouldBe(recTime.Seconds);
        packet.Header.TransmitTimestamp.Value.Seconds.ShouldBe(xmtTime.Seconds);
    }

    [Fact]
    public void Parse_WithFullHeader_ParsesRootDelayAndRootDispersion()
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        response[1] = 1;

        var delay = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(100));
        delay.Encode().CopyTo(response, 4);

        var dispersion = NtpShort.FromTimeSpan(TimeSpan.FromSeconds(50));
        dispersion.Encode().CopyTo(response, 8);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        ((int)packet.Header.RootDelay.Value.Seconds).ShouldBe((int)delay.Seconds);
        ((int)packet.Header.RootDispersion.Value.Seconds).ShouldBe((int)dispersion.Seconds);
    }

    [Fact]
    public void Parse_WithFullHeader_ParsesReferenceId()
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        response[1] = 1;

        var refIdBytes = System.Text.Encoding.ASCII.GetBytes("GPS ");
        refIdBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.ReferenceId.Value.ShouldBe("GPS ");
    }

    [Fact]
    public void KissCode_WithNonZeroStratum_ReturnsNull()
    {
        var response = CreateResponse();
        response[1] = 1;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.KissCode.ShouldBeNull();
    }

    [Fact]
    public void KissCode_WithZeroStratum_ReturnsKissCode()
    {
        var response = CreateResponse();
        response[1] = 0;

        var kissBytes = System.Text.Encoding.ASCII.GetBytes("DENY");
        kissBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.KissCode.ShouldNotBeNull();
        packet.Header.KissCode.Value.ShouldBe("DENY");
    }

    [Fact]
    public void KissCode_WithZeroStratumAndInitCode_ReturnsInit()
    {
        var response = CreateResponse();
        response[1] = 0;

        var kissBytes = System.Text.Encoding.ASCII.GetBytes("INIT");
        kissBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.KissCode.ShouldNotBeNull();
        packet.Header.KissCode.Value.ShouldBe("INIT");
    }

    [Fact]
    public void ValidateKissODeath_WithActionableKissCode_Throws()
    {
        var response = CreateResponse();
        response[1] = 0;

        var kissBytes = System.Text.Encoding.ASCII.GetBytes("DENY");
        kissBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        Should.Throw<NtpKissODeathException>(() => packet.Header.ValidateKissODeath());
    }

    [Fact]
    public void ValidateKissODeath_WithRstrKissCode_Throws()
    {
        var response = CreateResponse();
        response[1] = 0;

        var kissBytes = System.Text.Encoding.ASCII.GetBytes("RSTR");
        kissBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        Should.Throw<NtpKissODeathException>(() => packet.Header.ValidateKissODeath());
    }

    [Fact]
    public void ValidateKissODeath_WithRateKissCode_Throws()
    {
        var response = CreateResponse();
        response[1] = 0;

        var kissBytes = System.Text.Encoding.ASCII.GetBytes("RATE");
        kissBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        Should.Throw<NtpKissODeathException>(() => packet.Header.ValidateKissODeath());
    }

    [Fact]
    public void ValidateKissODeath_WithInformationalKissCode_DoesNotThrow()
    {
        var response = CreateResponse();
        response[1] = 0;

        var kissBytes = System.Text.Encoding.ASCII.GetBytes("INIT");
        kissBytes.CopyTo(response, 12);

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.ValidateKissODeath();
    }

    [Fact]
    public void ValidateKissODeath_WithNonZeroStratum_DoesNotThrow()
    {
        var response = CreateResponse();
        response[1] = 1;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.ValidateKissODeath();
    }

    [Fact]
    public void ValidateOriginTimestamp_WithMatchingTransmitTimestamp_DoesNotThrow()
    {
        var requestTransmitTimestamp = NtpTimestamp.FromDateTime(
            new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var response = CreateResponseWithOriginTimestamp(requestTransmitTimestamp);
        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.ValidateOriginTimestamp(TransmitTimestamp.Reconstitute(requestTransmitTimestamp));
    }

    [Fact]
    public void ValidateOriginTimestamp_WithZeroOriginTimestamp_Throws()
    {
        var requestTransmitTimestamp = NtpTimestamp.FromDateTime(
            new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var response = CreateResponseWithOriginTimestamp(NtpTimestamp.Zero);
        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        Should.Throw<ApplicationException>(() =>
            packet.Header.ValidateOriginTimestamp(TransmitTimestamp.Reconstitute(requestTransmitTimestamp)));
    }

    [Fact]
    public void ValidateOriginTimestamp_WithMismatchedOriginTimestamp_Throws()
    {
        var requestTransmitTimestamp = NtpTimestamp.FromDateTime(
            new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var response = CreateResponseWithOriginTimestamp(requestTransmitTimestamp);
        NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 1, DateTimeKind.Utc))
            .Encode()
            .CopyTo(response, 24);
        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        Should.Throw<ApplicationException>(() =>
            packet.Header.ValidateOriginTimestamp(TransmitTimestamp.Reconstitute(requestTransmitTimestamp)));
    }

    [Fact]
    public void ValidateOriginTimestamp_WithBroadcastModeAndZeroOriginTimestamp_DoesNotThrow()
    {
        var requestTransmitTimestamp = NtpTimestamp.FromDateTime(
            new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var response = CreateResponseWithOriginTimestamp(NtpTimestamp.Zero);
        response[0] = 0b_00_100_101;
        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.ValidateOriginTimestamp(TransmitTimestamp.Reconstitute(requestTransmitTimestamp));
    }

    [Fact]
    public void Parse_WithDestinationTimestamp_SetsDestinationTimestamp()
    {
        var response = CreateResponse();
        var destTime = NtpTimestamp.FromDateTime(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));

        var packet = ReceivePacketHeader.Parse(response, destTime);

        packet.DestinationTimestamp.ShouldNotBeNull();
        packet.DestinationTimestamp!.Seconds.ShouldBe(destTime.Seconds);
    }

    [Fact]
    public void KissODeath_WithZeroStratum_ReturnsTrue()
    {
        var response = CreateResponse();
        response[1] = 0;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.KissODeath.ShouldBeTrue();
    }

    [Fact]
    public void KissODeath_WithNonZeroStratum_ReturnsFalse()
    {
        var response = CreateResponse();
        response[1] = 1;

        var packet = ReceivePacketHeader.Parse(response, NtpTimestamp.Zero);

        packet.Header.KissODeath.ShouldBeFalse();
    }

    private static byte[] CreateResponse()
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        return response;
    }

    private static byte[] CreateResponseWithOriginTimestamp(NtpTimestamp originTimestamp)
    {
        var response = CreateResponse();
        originTimestamp.Encode().CopyTo(response, 24);
        return response;
    }
}
