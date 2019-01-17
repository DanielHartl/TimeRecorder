using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace ActivityTracker.Server.Persistence.AzureStorage
{
    internal class TimeRangeCollectionEntity : TableEntity
    {
        public DateTimeOffset StartDate { get; set; }
        public string TimeRangesJson { get; set; }
    }

    internal class TimeRangeEntity
    {
        [JsonProperty("s")]
        public DateTime StartTime { get; set; }
        [JsonProperty("e")]
        public DateTime EndTime { get; set; }
    }
}