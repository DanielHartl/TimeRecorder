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
        public ActivitySummaryResponse ToResponse(ActivitySummary activitySummary)
        {
            return new ActivitySummaryResponse(
                activitySummary.WeeklySummaries.Select(ToWeeklyModel).ToArray());
        }

        private WeeklySummaryModel ToWeeklyModel(WeeklySummary weeklySummary)
        {
            return new WeeklySummaryModel(
                weeklySummary.WeekStart.ToString("MM dd"),
                weeklySummary.DailySummaries.Select(ToDailyModel).ToArray(),
                weeklySummary.TotalDuration.ToString("hh\\:mm"));
        }

        private DailySummaryModel ToDailyModel(DailySummary dailySummary)
        {
            return new DailySummaryModel(
                dailySummary.Day.ToString("MMM dd"),
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
