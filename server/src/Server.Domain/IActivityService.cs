using System;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Domain
{
    public interface IActivityService
    {
        Task ReportEventAsync(string user, DateTimeOffset eventTime);

        Task<ActivitySummary> GetActivitySummaryAsync(string user, TimeSpan timeZoneOffset);
    }
}