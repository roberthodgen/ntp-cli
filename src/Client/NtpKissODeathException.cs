namespace RobertHodgen.Ntp.Client;

/// <summary>
/// Exception thrown when the NTP server responds with a Kiss-o'-Death packet that requires client action.
/// </summary>
public sealed class NtpKissODeathException : Exception
{
    /// <summary>
    /// Gets the kiss code from the server's Kiss-o'-Death packet.
    /// </summary>
    public KissCodes KissCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NtpKissODeathException"/> class with the specified kiss code.
    /// </summary>
    /// <param name="kissCode">The kiss code received from the NTP server.</param>
    public NtpKissODeathException(KissCodes kissCode)
        : base($"Received NTP Kiss-o'-Death packet with kiss code '{kissCode.Value}'.")
    {
        KissCode = kissCode;
    }
}
