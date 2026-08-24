namespace RobertHodgen.Ntp.Client.Remote;

using System.Diagnostics;
using Fields;

public sealed class MonotonicClock : IMonotonicClock
{
    private const ulong NtpFractionDivisor = 4294967296ul;
    private static readonly double NtpFractionPerTick = (double)NtpFractionDivisor / Stopwatch.Frequency;

    private readonly DateTime _referenceTime;
    private readonly uint _referenceSeconds;
    private readonly uint _referenceFraction;
    private readonly Stopwatch _stopwatch;

    public MonotonicClock()
    {
        #region Time Sensitive

        _referenceTime = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();

        #endregion

        var ntp = NtpTimestamp.FromDateTime(_referenceTime);
        _referenceSeconds = ntp.Seconds;
        _referenceFraction = ntp.Fraction;
    }

    public DateTime UtcNow => _referenceTime + _stopwatch.Elapsed;

    public NtpTimestamp Capture()
    {
        var elapsedTicks = _stopwatch.ElapsedTicks;
        var elapsedSeconds = (uint)(elapsedTicks / Stopwatch.Frequency);
        var remainingTicks = elapsedTicks % Stopwatch.Frequency;
        var elapsedFraction = (uint)(remainingTicks * NtpFractionPerTick);

        var totalFraction = (ulong)_referenceFraction + elapsedFraction;
        var carrySeconds = totalFraction / NtpFractionDivisor;
        var finalFraction = (uint)(totalFraction % NtpFractionDivisor);

        var finalSeconds = _referenceSeconds + elapsedSeconds + (uint)carrySeconds;

        return NtpTimestamp.Create(finalSeconds, finalFraction);
    }
}
