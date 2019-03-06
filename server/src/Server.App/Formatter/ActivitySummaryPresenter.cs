using System.Linq;
using TimeRecorder.Server.App.Contracts;
using TimeRecorder.Server.Domain;

namespace TimeRecorder.Server.App.Formatter
{
    public interface IActivitySummaryFormatter
    {
        ActivitySummaryResponse ToResponse(ActivitySummary activitySummary);
    }

    public class DefaultActivitySummaryFormatter : IActivitySummaryFormatter
    {
        private const string DateTimeFormatString = "yyyy-MM-ddTHH:mm:ss";

        private const string HourlyFormatString = "HH\\:mm";

        public ActivitySummaryResponse ToResponse(ActivitySummary activitySummary)
        {
            return new ActivitySummaryResponse(
                activitySummary.DailySummaries.Select(ToDailyModel).ToArray());
        }

        private DailySummaryModel ToDailyModel(DailySummary dailySummary)
        {
            return new DailySummaryModel(
                dailySummary.Day.ToString(DateTimeFormatString),
                (int)dailySummary.TotalDuration.TotalMinutes,
                dailySummary.TimeRanges.Select(ToTimeRangeModel).ToArray());
        }

        private TimeRangeModel ToTimeRangeModel(TimeRangeLocal timeRangeLocal)
        {
            return new TimeRangeModel(
                timeRangeLocal.Start.ToString(HourlyFormatString),
                timeRangeLocal.End.ToString(HourlyFormatString),
                (int)timeRangeLocal.Duration.TotalMinutes);
        }
    }
}
