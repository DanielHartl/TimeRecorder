using System;

namespace TimeRecorder.Server.Domain
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset Floor(this DateTimeOffset dateTimeOffset, TimeSpan span)
        {
            var ticks = dateTimeOffset.Ticks / span.Ticks;
            return new DateTimeOffset(ticks * span.Ticks, dateTimeOffset.Offset);
        }

        public static DateTimeOffset Ceil(this DateTimeOffset dateTimeOffset, TimeSpan span)
        {
            var ticks = (dateTimeOffset.Ticks + span.Ticks - 1) / span.Ticks;
            return new DateTimeOffset(ticks * span.Ticks, dateTimeOffset.Offset);
        }

        public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}