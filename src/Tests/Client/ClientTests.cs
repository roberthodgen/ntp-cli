namespace Roberthodgen.Ntp.Client.Tests;

using System.Net;
using System.Text;
using RobertHodgen.Ntp.Client;

public class ClientTests
{
    [Fact]
    public async Task InitializeClientAsync_UsesConfiguredHostForDnsResolution()
    {
        string? resolvedHost = null;
        var sut = new RobertHodgen.Ntp.Client.Client(
            "time.example.test",
            (host, _) =>
            {
                resolvedHost = host;
                return Task.FromResult(new[] { IPAddress.Loopback });
            });

        await sut.InitializeClientAsync();

        resolvedHost.ShouldBe("time.example.test");
    }

    [Theory]
    [InlineData("DENY")]
    [InlineData("RSTR")]
    [InlineData("RATE")]
    public void ReadResponse_WithActionableKissCode_Throws(string kissCode)
    {
        var response = CreateKissOfDeathResponse(kissCode);

        var exception = Should.Throw<NtpKissODeathException>(() => RobertHodgen.Ntp.Client.Client.ReadResponse(48, response));

        exception.KissCode.Value.ShouldBe(kissCode);
    }

    [Fact]
    public void ReadResponse_WithInformationalKissCode_DoesNotThrow()
    {
        var response = CreateKissOfDeathResponse("INIT");

        var packet = RobertHodgen.Ntp.Client.Client.ReadResponse(48, response);

        packet.Header.KissCode.ShouldNotBeNull();
        packet.Header.KissCode.Value.ShouldBe("INIT");
    }

    private static Memory<byte> CreateKissOfDeathResponse(string kissCode)
    {
        var response = new byte[48];
        response[0] = 0b_00_100_100;
        response[1] = 0;
        Encoding.ASCII.GetBytes(kissCode).CopyTo(response, 12);
        return response;
    }
}