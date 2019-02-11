using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ActivityTracker.Server.Domain
{
    public struct ActivitySummary
    {
        public IEnumerable<WeeklySummary> WeeklySummaries { get; }

        public ActivitySummary(IEnumerable<WeeklySummary> weeklySummaries)
        {
            WeeklySummaries = weeklySummaries;
        }
    }

    public struct WeeklySummary
    {
        public DateTime WeekStart { get; }

        public IEnumerable<DailySummary> DailySummaries { get; }

        public TimeSpan TotalDuration => TimeSpan.FromTicks(DailySummaries.Sum(x => x.TotalDuration.Ticks));

        public WeeklySummary(DateTime weekStart, IEnumerable<DailySummary> dailySummaries)
        {
            WeekStart = weekStart;
            DailySummaries = dailySummaries ?? throw new ArgumentNullException(nameof(dailySummaries));
        }
    }

    public struct TimeRangeLocal
    {
        public DateTime Start { get; }
        public DateTime End => Start.Add(Duration);
        public TimeSpan Duration { get; }

        public TimeRangeLocal(DateTime start, TimeSpan duration)
        {
            Start = start;
            Duration = duration;
        }
    }

    public struct DailySummary
    {
        public DateTime Day { get; }

        public IEnumerable<TimeRangeLocal> TimeRanges { get; }

        public TimeSpan TotalDuration => TimeSpan.FromTicks(TimeRanges.Sum(x => x.Duration.Ticks));

        public DailySummary(DateTime day, IEnumerable<TimeRangeLocal> timeRanges)
        {
            Day = day;
            TimeRanges = timeRanges ?? throw new ArgumentNullException(nameof(timeRanges));
        }
    }

    public class EventRecord
    {
        public DateTimeOffset BaseTime { get; }
        private readonly ConcurrentDictionary<DateTimeOffset, int> _events = new ConcurrentDictionary<DateTimeOffset, int>();
        public IEnumerable<KeyValuePair<DateTimeOffset, int>> Events => _events.OrderBy(x => x.Key);

        public EventRecord(DateTimeOffset baseTime)
        {
            BaseTime = baseTime;
        }

        public void AddEvents(IEnumerable<KeyValuePair<DateTimeOffset, int>> newEvents)
        {
            foreach (var (key, value) in newEvents)
            {
                var currentBase = key.Floor(TimeSpan.FromHours(1));
                if (currentBase != BaseTime)
                {
                    throw new ArgumentException("Invalid timestamp");
                }

                _events.AddOrUpdate(key, x => value, (k, v) => v + value);
            }
        }
    }

    public class TimeRangeCollection
    {
        private readonly TimeSpan _datetimeResolution = TimeSpan.FromSeconds(1);

        private readonly IList<TimeRange> _timeRanges;

        public TimeRangeCollection(DateTimeOffset startDate, IEnumerable<TimeRange> timeRanges = null)
        {
            StartDate = startDate.StartOfWeek();
            _timeRanges = NormalizeTimeRanges(timeRanges).ToList();
        }

        public DateTimeOffset StartDate { get; }

        public IEnumerable<TimeRange> TimeRanges => _timeRanges;

        public void AddEvent(DateTimeOffset eventTime, TimeSpan toleranceWindow)
        {
            eventTime = eventTime.Floor(_datetimeResolution);

            var previousRange = _timeRanges.LastOrDefault(x => x.Start < eventTime);

            if (previousRange == null || previousRange.End.Add(toleranceWindow) < eventTime)
            {
                var nextRange = _timeRanges.FirstOrDefault(x => x.Start >= eventTime);

                if (nextRange != null && nextRange.Start.Subtract(toleranceWindow) < eventTime)
                {
                    nextRange.SetStartTime(eventTime);
                }
                else
                {
                    var insertIndex = GetInsertIndex(eventTime);
                    _timeRanges.Insert(insertIndex, new TimeRange(eventTime, eventTime));
                }
            }
            else if (previousRange.End < eventTime)
            {
                previousRange.SetEndTime(eventTime);
            }
        }

        private int GetInsertIndex(DateTimeOffset eventTime)
        {
            for (var i = 0; i < _timeRanges.Count; i++)
            {
                if (eventTime < _timeRanges[i].Start)
                {
                    return i;
                }
            }

            return _timeRanges.Count;
        }

        private IEnumerable<TimeRange> NormalizeTimeRanges(IEnumerable<TimeRange> timeRanges)
        {
            if (timeRanges == null) yield break;

            DateTimeOffset? lastEnd = null;

            foreach (var timeRange in timeRanges.OrderBy(x => x.Start))
            {
                if (timeRange.Start.StartOfWeek() != StartDate)
                {
                    throw new ArgumentException($"Invalid time range {timeRange.Start} not in week of {StartDate}");
                }

                if (lastEnd.HasValue && lastEnd.Value > timeRange.Start)
                {
                    throw new ArgumentException($"{timeRange.Start} is before {lastEnd.Value}");
                }

                yield return timeRange;
                lastEnd = timeRange.End;
            }
        }
    }
}