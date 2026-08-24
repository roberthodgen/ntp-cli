namespace RobertHodgen.Ntp.Client.Remote;

using Fields;

/// <summary>
/// Provides access to the current UTC time and the ability to capture NTP timestamps with monotonic precision.
/// </summary>
public interface IMonotonicClock
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Captures the current time as an NTP timestamp using a monotonic clock source.
    /// </summary>
    /// <returns>The current time as an NTP timestamp.</returns>
    NtpTimestamp Capture();
}
