using System;

namespace ActivityTracker.Server.Domain
{
    public class TimeRange
    {
        public TimeRange(DateTimeOffset start, DateTimeOffset end)
        {
            Validate(start, end);
            Start = start;
            End = end;
        }

        public DateTimeOffset Start { get; private set; }
        public DateTimeOffset End { get; private set; }

        public void SetStartTime(DateTimeOffset start)
        {
            Validate(start, End);
            Start = start;
        }

        public void SetEndTime(DateTimeOffset end)
        {
            Validate(Start, end);
            End = end;
        }

        private static void Validate(DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end) throw new ArgumentException($"{start} has to be before {end}");
        }

        public override string ToString()
        {
            return $"{Start} -> {End}";
        }

        public TimeSpan Duration => End - Start;
    }
}
