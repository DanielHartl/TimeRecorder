using System;

namespace ActivityTracker.Server.Domain
{
    public static class DateTimeOffsetExtensions
    {
        private static DateTimeOffset Floor(this DateTimeOffset dateTimeOffset, TimeSpan precision)
        {
            return new DateTimeOffset(
                dateTimeOffset.Ticks - dateTimeOffset.Ticks % precision.Ticks,
                dateTimeOffset.Offset);
        }
    }
}