using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ActivityTracker.Server.App.Contracts
{
    public class ActivitySummaryResponse
    {
        [JsonProperty("weekly")]
        public WeeklySummaryModel[] WeeklySummaryModels { get; }

        public ActivitySummaryResponse(WeeklySummaryModel[] weeklySummaryModels)
        {
            WeeklySummaryModels = weeklySummaryModels ?? throw new ArgumentNullException(nameof(weeklySummaryModels));
        }
    }

    public class WeeklySummaryModel
    {
        [JsonProperty("weekstart")]
        public string WeekStart { get; }

        [JsonProperty("daily")]
        public DailySummaryModel[] DailySummaryModels { get; }

        public WeeklySummaryModel(string weekStart, DailySummaryModel[] dailySummaryModels)
        {
            WeekStart = weekStart ?? throw new ArgumentNullException(nameof(weekStart));
            DailySummaryModels = dailySummaryModels ?? throw new ArgumentNullException(nameof(dailySummaryModels));
        }
    }

    public class DailySummaryModel
    {
        public string Day { get; }
        public string TotalDuration { get; }

        [JsonProperty("timeranges")]
        public TimeRangeModel[] TimeRangeModels { get; }

        public DailySummaryModel(
            string day,
            string totalDuration,
            TimeRangeModel[] timeRangeModels)
        {
            Day = day;
            TotalDuration = totalDuration;
            TimeRangeModels = timeRangeModels ?? throw new ArgumentNullException(nameof(timeRangeModels));
        }
    }

    public class TimeRangeModel
    {
        [JsonProperty("start")]
        public string Start { get; }
        [JsonProperty("end")]
        public string End { get; }
        [JsonProperty("duration")]
        public string Duration { get; }

        public TimeRangeModel(string start, string end, string duration)
        {
            Start = start ?? throw new ArgumentNullException(nameof(start));
            End = end ?? throw new ArgumentNullException(nameof(end));
            Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        }
    }

    public class ReportEventRequest
    {
        public ReportEventRequest(string tag, DateTimeOffset eventTime)
        {
            Tag = tag;
            EventTime = eventTime;
        }

        [Required]
        public string Tag { get; }
        [Required]
        public DateTimeOffset EventTime { get; }
    }
}
