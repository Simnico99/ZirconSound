using System;

namespace ZirconSound.Extensions
{
    internal static class TimeSpanExtension
    {
        public static TimeSpan StripMilliseconds(this TimeSpan time) => new(time.Days, time.Hours, time.Minutes, time.Seconds);
    }
}