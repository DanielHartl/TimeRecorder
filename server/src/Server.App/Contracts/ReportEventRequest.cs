using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TimeRecorder.Server.App.Contracts
{
    public class ActivitySummaryResponse
    {
        [JsonProperty("daily")]
        public DailySummaryModel[] DailySummaryModels { get; }

        public ActivitySummaryResponse(DailySummaryModel[] dailySummaryModels)
        {
            DailySummaryModels = dailySummaryModels ?? throw new ArgumentNullException(nameof(dailySummaryModels));
        }
    }

    public class DailySummaryModel
    {
        [JsonProperty("day")]
        public string Day { get; }

        [JsonProperty("total_duration")]
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
        public ReportEventRequest(string user, IDictionary<DateTimeOffset, int> events)
        {
            User = user;
            Events = events;
        }

        [Required]
        public string User { get; }
        [Required]
        public IDictionary<DateTimeOffset, int> Events { get; }
    }
}
