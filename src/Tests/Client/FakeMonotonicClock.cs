namespace Roberthodgen.Ntp.Client.Tests;

using RobertHodgen.Ntp.Client.Remote;
using RobertHodgen.Ntp.Client.Remote.Fields;

public class FakeMonotonicClock : IMonotonicClock
{
    public DateTime Time { get; set; } = new(2026, 8, 23, 12, 0, 0, DateTimeKind.Utc);

    public DateTime UtcNow => Time;
    public NtpTimestamp Capture() => NtpTimestamp.FromDateTime(Time);
}
