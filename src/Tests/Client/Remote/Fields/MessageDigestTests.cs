namespace Roberthodgen.Ntp.Client.Tests.Remote.Fields;

using System.Text;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class MessageDigestTests
{
    [Fact]
    public void None_HasEmptyValue()
    {
        MessageDigest.None.Value.Length.ShouldBe(0);
    }

    [Fact]
    public void None_Encode_ProducesEmptyArray()
    {
        var bytes = MessageDigest.None.Encode();
        bytes.Length.ShouldBe(0);
    }

    [Fact]
    public void CreateNew_ComputesMd5Hash()
    {
        var input = Encoding.ASCII.GetBytes("test data");
        var digest = MessageDigest.CreateNew(input);

        digest.Value.Length.ShouldBe(16);
    }

    [Fact]
    public void CreateNew_WithSameInput_ProducesSameHash()
    {
        var input = Encoding.ASCII.GetBytes("test data");
        var digest1 = MessageDigest.CreateNew(input);
        var digest2 = MessageDigest.CreateNew(input);

        digest1.Value.ShouldBe(digest2.Value);
    }

    [Fact]
    public void CreateNew_WithDifferentInput_ProducesDifferentHash()
    {
        var input1 = Encoding.ASCII.GetBytes("test data 1");
        var input2 = Encoding.ASCII.GetBytes("test data 2");
        var digest1 = MessageDigest.CreateNew(input1);
        var digest2 = MessageDigest.CreateNew(input2);

        digest1.Value.ShouldNotBe(digest2.Value);
    }

    [Fact]
    public void CreateNew_AlwaysProducesSixteenByteHash()
    {
        var input = new byte[] { 1, 2, 3 };
        var digest = MessageDigest.CreateNew(input);
        digest.Value.Length.ShouldBe(16);
    }
}
