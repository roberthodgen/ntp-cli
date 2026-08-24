namespace Roberthodgen.Ntp.Client.Tests.Remote;

using RobertHodgen.Ntp.Client.Remote;

public class MonotonicClockTests
{
    [Fact]
    public void UtcNow_ReturnsTimeCloseToSystemUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var clock = new MonotonicClock();
        var after = DateTime.UtcNow.AddSeconds(1);

        var utcNow = clock.UtcNow;

        utcNow.ShouldBeGreaterThanOrEqualTo(before);
        utcNow.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public void Capture_ReturnsNonZeroTimestamp()
    {
        var clock = new MonotonicClock();
        var ts = clock.Capture();

        ts.Seconds.ShouldNotBe(0u);
    }

    [Fact]
    public void Capture_MultipleCalls_AreMonotonicallyIncreasing()
    {
        var clock = new MonotonicClock();
        var first = clock.Capture();
        System.Threading.Thread.Sleep(1);
        var second = clock.Capture();

        var combined = (long)second.Seconds << 32 | second.Fraction;
        var firstCombined = (long)first.Seconds << 32 | first.Fraction;
        combined.ShouldBeGreaterThan(firstCombined);
    }

    [Fact]
    public void Capture_RoundTripsWithinOneMillisecond()
    {
        var clock = new MonotonicClock();
        var ts = clock.Capture();

        var diff = (ts.ToDateTime() - clock.UtcNow).Duration();
        diff.ShouldBeLessThanOrEqualTo(TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void UtcNow_AfterDelay_ReflectsElapsedTicks()
    {
        var clock = new MonotonicClock();
        var initial = clock.UtcNow;
        System.Threading.Thread.Sleep(10);
        var later = clock.UtcNow;

        (later - initial).TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(8);
    }
}
