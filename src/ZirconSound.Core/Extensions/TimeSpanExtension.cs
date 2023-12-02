namespace ZirconSound.Core.Extensions;
public static class TimeSpanExtension
{
    public static TimeSpan StripMilliseconds(this TimeSpan time) => new(time.Days, time.Hours, time.Minutes, time.Seconds);
}
