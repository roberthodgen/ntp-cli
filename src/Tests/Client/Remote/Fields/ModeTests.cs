namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class ModeTests
{
    [Fact]
    public void SizeInBits_IsThree()
    {
        Mode.Client.SizeInBits.ShouldBe(3);
    }

    [Theory]
    [InlineData(0, "reserved")]
    [InlineData(1, "symmetric active")]
    [InlineData(2, "symmetric passive")]
    [InlineData(3, "client")]
    [InlineData(4, "server")]
    [InlineData(5, "broadcast")]
    [InlineData(6, "NTP control message")]
    [InlineData(7, "reserved for private use")]
    public void ToString_WithKnownValue_ReturnsExpectedLabel(byte value, string expected)
    {
        Mode.Reconstitute(value).ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Reconstitute_RoundTrip_PreservesValue(byte value)
    {
        var mode = Mode.Reconstitute(value);
        ((int)mode.Value).ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Encode_ProducesSingleByteWithCorrectValue(byte value)
    {
        var mode = Mode.Reconstitute(value);
        var bytes = mode.Encode();

        bytes.Length.ShouldBe(1);
        ((int)bytes[0]).ShouldBe(value);
    }

    [Fact]
    public void Reserved_HasValueZero() => ((int)Mode.Reserved.Value).ShouldBe(0);

    [Fact]
    public void SymmetricActive_HasValueOne() => ((int)Mode.SymmetricActive.Value).ShouldBe(1);

    [Fact]
    public void SymmetricPassive_HasValueTwo() => ((int)Mode.SymmetricPassive.Value).ShouldBe(2);

    [Fact]
    public void Client_HasValueThree() => ((int)Mode.Client.Value).ShouldBe(3);

    [Fact]
    public void Server_HasValueFour() => ((int)Mode.Server.Value).ShouldBe(4);

    [Fact]
    public void Broadcast_HasValueFive() => ((int)Mode.Broadcast.Value).ShouldBe(5);

    [Fact]
    public void NtpControlMessage_HasValueSix() => ((int)Mode.NtpControlMessage.Value).ShouldBe(6);

    [Fact]
    public void ReservedForPrivateUse_HasValueSeven() => ((int)Mode.ReservedForPrivateUse.Value).ShouldBe(7);

    [Fact]
    public void ToString_WithOutOfRangeValue_ThrowsApplicationException()
    {
        var mode = Mode.Reconstitute(8);
        Should.Throw<ApplicationException>(() => mode.ToString());
    }
}
