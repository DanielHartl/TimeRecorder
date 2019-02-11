using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Domain
{
    public interface IActivityService
    {
        Task ReportEventAsync(string user, IDictionary<DateTimeOffset, int> events);

        Task<ActivitySummary> GetActivitySummaryAsync(string user, TimeSpan timeZoneOffset);
    }
}