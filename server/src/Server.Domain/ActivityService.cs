using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Domain
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;
        private readonly TimeSpan _toleranceWindow;
        private readonly IClock _clock;

        public ActivityService(IActivityRepository activityRepository, TimeSpan toleranceWindow, IClock clock = null)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            _toleranceWindow = toleranceWindow;
            _clock = clock ?? new SystemClock();
        }

        public async Task ReportEventAsync(string user, DateTimeOffset eventTime)
        {
            var weekStart = eventTime.StartOfWeek();

            await _activityRepository.ReadAndCreateOrUpdateAsync(
                user,
                weekStart,
                timeRangeCollection => {
                    if (timeRangeCollection == null)
                    {
                        timeRangeCollection = new TimeRangeCollection(weekStart);
                    }

                    timeRangeCollection.AddEvent(eventTime, _toleranceWindow);
                    return timeRangeCollection;
                });
        }

        public async Task<ActivitySummary> GetActivitySummaryAsync(string user, TimeSpan timeZoneOffset)
        {
            if (timeZoneOffset.TotalHours < -14 || timeZoneOffset.TotalHours > 14)
            {
                throw new ArgumentException("Value out of range", nameof(timeZoneOffset));
            }

            var start = _clock.UtcNow.Date.AddDays(-22);
            var end = _clock.UtcNow.Date.AddDays(1);

            var timeRanges = await _activityRepository.GetTimeRangesAsync(user, start, end);

            return ToActivitySummary(
                timeRanges.SelectMany(x => x.TimeRanges)
                    .Select(x => new TimeRangeLocal(x.Start.ToOffset(timeZoneOffset).DateTime, x.Duration))
                    .OrderByDescending(x => x.Start.Date));
        }

        private static ActivitySummary ToActivitySummary(IEnumerable<TimeRangeLocal> timeRanges)
        {
            var groupByWeek = timeRanges.GroupBy(x => x.Start.StartOfWeek());
            return new ActivitySummary(
                from weekly in groupByWeek
                let groupByDay = weekly.GroupBy(x => x.Start.Date)
                select new WeeklySummary(weekly.Key, groupByDay.Select(daily => new DailySummary(daily.Key, daily))));
        }
    }
}