namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using System.Net;
using System.Text;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class ReferenceIdTests
{
    [Theory]
    [InlineData("DENY")]
    [InlineData("GPS ")]
    [InlineData("RATE")]
    [InlineData("INIT")]
    public void Parse_WithAsciiReferenceId_PreservesPacketOrder(string value)
    {
        var parsed = ReferenceId.Parse(Encoding.ASCII.GetBytes(value));

        parsed.Value.ShouldBe(value);
    }

    [Fact]
    public void SizeInBits_IsThirtyTwo()
    {
        ReferenceId.Empty.SizeInBits.ShouldBe(32);
    }

    [Theory]
    [InlineData("DENY")]
    [InlineData("GPS ")]
    [InlineData("RATE")]
    [InlineData("INIT")]
    [InlineData("    ")]
    public void Encode_WithAsciiReferenceId_ProducesAsciiBytes(string value)
    {
        var referenceId = ReferenceId.CreateNew(value);
        var bytes = referenceId.Encode();

        bytes.Length.ShouldBe(4);
        Encoding.ASCII.GetString(bytes).ShouldBe(value);
    }

    [Fact]
    public void Encode_Parse_RoundTrip_PreservesValue()
    {
        var referenceId = ReferenceId.CreateNew("GPS ");
        var bytes = referenceId.Encode();
        var parsed = ReferenceId.Parse(bytes);

        parsed.Value.ShouldBe(referenceId.Value);
    }

    [Fact]
    public void CreateNewFromIpAddress_WithValidIPv4Address_ProducesCorrectReferenceId()
    {
        var address = IPAddress.Parse("192.168.1.1");
        var referenceId = ReferenceId.CreateNewFromIpAddress(address);

        referenceId.Value.ShouldBe(Encoding.ASCII.GetString(address.GetAddressBytes()));
    }

    [Fact]
    public void CreateNewFromIpAddress_WithIPv6Address_ThrowsArgumentException()
    {
        var address = IPAddress.Parse("::1");

        Should.Throw<ArgumentException>(() => ReferenceId.CreateNewFromIpAddress(address))
            .ParamName.ShouldBe("address");
    }

    [Fact]
    public void CreateNew_WithLessThanFourCharacters_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => ReferenceId.CreateNew("ABC"))
            .ParamName.ShouldBe("value");
    }

    [Fact]
    public void CreateNew_WithMoreThanFourCharacters_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => ReferenceId.CreateNew("ABCDE"))
            .ParamName.ShouldBe("value");
    }

    [Fact]
    public void Parse_WithWrongLength_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => ReferenceId.Parse(new byte[3]))
            .ParamName.ShouldBe("memory");

        Should.Throw<ArgumentException>(() => ReferenceId.Parse(new byte[5]))
            .ParamName.ShouldBe("memory");
    }

    [Fact]
    public void Empty_HasFourSpaces()
    {
        ReferenceId.Empty.Value.ShouldBe("    ");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var referenceId = ReferenceId.CreateNew("GPS ");
        referenceId.ToString().ShouldBe("GPS ");
    }
}
