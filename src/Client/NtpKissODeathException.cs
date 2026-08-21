namespace RobertHodgen.Ntp.Client;

public sealed class NtpKissODeathException : Exception
{
    public KissCodes KissCode { get; }

    public NtpKissODeathException(KissCodes kissCode)
        : base($"Received NTP Kiss-o'-Death packet with kiss code '{kissCode.Value}'.")
    {
        KissCode = kissCode;
    }
}