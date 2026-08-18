namespace Roberthodgen.Ntp.Client.Tests;

public class KissCodesTests
{
    [Fact]
    public void CreateNew_WithFourCharacterCode_SetsValue()
    {
        var kissCode = KissCodes.CreateNew("RATE");

        kissCode.Value.ShouldBe("RATE");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("ABCDE")]
    public void CreateNew_WithCodeThatIsNotFourCharacters_Throws(string value)
    {
        Should.Throw<ArgumentException>(() => KissCodes.CreateNew(value))
            .ParamName.ShouldBe("value");
    }
}
