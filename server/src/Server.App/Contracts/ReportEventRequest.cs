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

        [JsonProperty("total_duration_in_minutes")]
        public int TotalDurationInMinutes { get; }

        [JsonProperty("timeranges")]
        public TimeRangeModel[] TimeRangeModels { get; }

        public DailySummaryModel(
            string day,
            int totalDurationInMinutes,
            TimeRangeModel[] timeRangeModels)
        {
            Day = day;
            TotalDurationInMinutes = totalDurationInMinutes;
            TimeRangeModels = timeRangeModels ?? throw new ArgumentNullException(nameof(timeRangeModels));
        }
    }

    public class TimeRangeModel
    {
        [JsonProperty("start")]
        public string Start { get; }
        [JsonProperty("end")]
        public string End { get; }
        [JsonProperty("duration_in_minutes")]
        public int DurationInMinutes { get; }

        public TimeRangeModel(string start, string end, int durationInMinutes)
        {
            Start = start ?? throw new ArgumentNullException(nameof(start));
            End = end ?? throw new ArgumentNullException(nameof(end));
            DurationInMinutes = durationInMinutes;
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
