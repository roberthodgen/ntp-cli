namespace RobertHodgen.Ntp.Client.Remote;

using System.Diagnostics;
using Fields;

/// <summary>
/// High-resolution monotonic clock implementation that captures NTP timestamps with sub-millisecond precision.
/// </summary>
/// <remarks>
/// Uses <see cref="Stopwatch"/> to measure elapsed time from a reference point,
/// avoiding discontinuities caused by system clock adjustments.
/// </remarks>
public sealed class MonotonicClock : IMonotonicClock
{
    private const ulong NtpFractionDivisor = 4294967296ul;
    private static readonly double NtpFractionPerTick = (double)NtpFractionDivisor / Stopwatch.Frequency;

    private readonly NtpTimestamp _referenceTime;
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonotonicClock"/> class, capturing the current time as a reference point.
    /// </summary>
    public MonotonicClock()
    {
        #region Time Sensitive

        var now = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();

        #endregion

        _referenceTime = NtpTimestamp.FromDateTime(now);
    }

    /// <inheritdoc />
    public DateTime UtcNow => _referenceTime.ToDateTime() + _stopwatch.Elapsed;

    /// <inheritdoc />
    public NtpTimestamp Capture()
    {
        var elapsedTicks = _stopwatch.ElapsedTicks;
        var elapsedSeconds = (uint)(elapsedTicks / Stopwatch.Frequency);
        var remainingTicks = elapsedTicks % Stopwatch.Frequency;
        var elapsedFraction = (uint)(remainingTicks * NtpFractionPerTick);

        var totalFraction = (ulong)_referenceTime.Fraction + elapsedFraction;
        var carrySeconds = totalFraction / NtpFractionDivisor;
        var finalFraction = (uint)(totalFraction % NtpFractionDivisor);

        var finalSeconds = _referenceTime.Seconds + elapsedSeconds + (uint)carrySeconds;

        return NtpTimestamp.Create(finalSeconds, finalFraction);
    }
}
