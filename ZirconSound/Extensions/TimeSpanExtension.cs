using System;

namespace ZirconSound.Extensions
{
    internal static class TimeSpanExtension
    {
        public static TimeSpan StripMilliseconds(this TimeSpan time)
        {
            return new(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
    }
}