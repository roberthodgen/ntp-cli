namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using RobertHodgen.Ntp.Client.Remote.Fields;

public class KeyIdTests
{
    [Fact]
    public void SizeInBits_IsThirtyTwo()
    {
        KeyId.None.SizeInBits.ShouldBe(32);
    }

    [Fact]
    public void None_HasValueZero()
    {
        ((long)KeyId.None.Value).ShouldBe(0L);
    }

    [Fact]
    public void None_Encode_ProducesFourZeroBytes()
    {
        var bytes = KeyId.None.Encode();

        bytes.Length.ShouldBe(4);
        bytes.ShouldAllBe(b => b == 0);
    }

    [Fact]
    public void Encode_ProducesNetworkByteOrder()
    {
        var bytes = KeyId.None.Encode();
        bytes.Length.ShouldBe(4);
    }
}
