namespace Roberthodgen.Ntp.Client.Tests;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class NtpKissODeathExceptionTests
{
    [Fact]
    public void Constructor_WithKissCode_SetsKissCodeProperty()
    {
        var kissCode = KissCodes.Deny;
        var exception = new NtpKissODeathException(kissCode);

        exception.KissCode.ShouldBe(kissCode);
    }

    [Fact]
    public void Constructor_WithKissCode_IncludesCodeInMessage()
    {
        var kissCode = KissCodes.RateExceeded;
        var exception = new NtpKissODeathException(kissCode);

        exception.Message.ShouldContain("RATE");
    }

    [Fact]
    public void Constructor_WithDenyKissCode_IncludesDenyInMessage()
    {
        var exception = new NtpKissODeathException(KissCodes.Deny);

        exception.Message.ShouldContain("DENY");
    }

    [Fact]
    public void Constructor_WithRstrKissCode_IncludesRstrInMessage()
    {
        var exception = new NtpKissODeathException(KissCodes.RestrictedAccess);

        exception.Message.ShouldContain("RSTR");
    }
}
