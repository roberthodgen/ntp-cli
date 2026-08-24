namespace RobertHodgen.Ntp.Client.Remote;

using System.Diagnostics;
using Fields;

public sealed class MonotonicClock : IMonotonicClock
{
    static readonly ulong NtpFractionDivisor = 4294967296ul;
    static readonly double NtpFractionPerTick = (double)NtpFractionDivisor / Stopwatch.Frequency;

    readonly DateTime _referenceTime;
    readonly uint _referenceSeconds;
    readonly uint _referenceFraction;
    readonly Stopwatch _stopwatch;

    public MonotonicClock()
    {
        _referenceTime = DateTime.UtcNow;
        var ntp = NtpTimestamp.FromDateTime(_referenceTime);
        _referenceSeconds = ntp.Seconds;
        _referenceFraction = ntp.Fraction;
        _stopwatch = Stopwatch.StartNew();
    }

    public DateTime UtcNow => _referenceTime + _stopwatch.Elapsed;

    public NtpTimestamp Capture()
    {
        var elapsedTicks = _stopwatch.ElapsedTicks;
        var elapsedSeconds = (uint)(elapsedTicks / (long)Stopwatch.Frequency);
        var remainingTicks = elapsedTicks % (long)Stopwatch.Frequency;
        var elapsedFraction = (uint)(remainingTicks * NtpFractionPerTick);

        var totalFraction = (ulong)_referenceFraction + elapsedFraction;
        var carrySeconds = totalFraction / NtpFractionDivisor;
        var finalFraction = (uint)(totalFraction % NtpFractionDivisor);

        var finalSeconds = _referenceSeconds + elapsedSeconds + (uint)carrySeconds;

        return NtpTimestamp.Create(finalSeconds, finalFraction);
    }
}
