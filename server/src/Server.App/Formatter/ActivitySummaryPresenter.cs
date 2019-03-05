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

        public ActivitySummaryResponse ToResponse(ActivitySummary activitySummary)
        {
            return new ActivitySummaryResponse(
                activitySummary.DailySummaries.Select(ToDailyModel).ToArray());
        }

        private DailySummaryModel ToDailyModel(DailySummary dailySummary)
        {
            return new DailySummaryModel(
                dailySummary.Day.ToString(DateTimeFormatString),
                dailySummary.TotalDuration.ToString("hh\\:mm"),
                dailySummary.TimeRanges.Select(ToTimeRangeModel).ToArray());
        }

        private TimeRangeModel ToTimeRangeModel(TimeRangeLocal timeRangeLocal)
        {
            return new TimeRangeModel(
                timeRangeLocal.Start.ToString("HH\\:mm"),
                timeRangeLocal.End.ToString("HH\\:mm"),
                timeRangeLocal.Duration.ToString("hh\\:mm"));
        }
    }
}
