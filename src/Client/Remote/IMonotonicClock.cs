namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

public interface IMonotonicClock
{
    DateTime UtcNow { get; }
    NtpTimestamp Capture();
}
