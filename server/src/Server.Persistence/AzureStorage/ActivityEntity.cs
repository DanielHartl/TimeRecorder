using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace ActivityTracker.Server.Persistence.AzureStorage
{
    internal class EventRecordEntity : TableEntity
    {
        public DateTime BaseDate { get; set; }
        public string Payload { get; set; }
    }

    internal class EventRecordMinuteEntry
    {
        [JsonProperty("m")]
        public int Minute { get; set; }
        [JsonProperty("c")]
        public int Count { get; set; }
    }
}