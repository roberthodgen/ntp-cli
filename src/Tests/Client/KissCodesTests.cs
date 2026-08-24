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

    [Fact]
    public void Deny_RequiresClientAction_IsTrue()
    {
        KissCodes.Deny.RequiresClientAction.ShouldBeTrue();
    }

    [Fact]
    public void RestrictedAccess_RequiresClientAction_IsTrue()
    {
        KissCodes.RestrictedAccess.RequiresClientAction.ShouldBeTrue();
    }

    [Fact]
    public void RateExceeded_RequiresClientAction_IsTrue()
    {
        KissCodes.RateExceeded.RequiresClientAction.ShouldBeTrue();
    }

    [Theory]
    [InlineData("ACST")]
    [InlineData("AUTH")]
    [InlineData("AUTO")]
    [InlineData("BCST")]
    [InlineData("CRYP")]
    [InlineData("DROP")]
    [InlineData("INIT")]
    [InlineData("MCST")]
    [InlineData("NKEY")]
    [InlineData("RMOT")]
    [InlineData("STEP")]
    public void InformationalKissCode_RequiresClientAction_IsFalse(string code)
    {
        var kissCode = KissCodes.CreateNew(code);
        kissCode.RequiresClientAction.ShouldBeFalse();
    }

    [Fact]
    public void Deny_Value_IsDENY()
    {
        KissCodes.Deny.Value.ShouldBe("DENY");
    }

    [Fact]
    public void RestrictedAccess_Value_IsRSTR()
    {
        KissCodes.RestrictedAccess.Value.ShouldBe("RSTR");
    }

    [Fact]
    public void RateExceeded_Value_IsRATE()
    {
        KissCodes.RateExceeded.Value.ShouldBe("RATE");
    }
}
