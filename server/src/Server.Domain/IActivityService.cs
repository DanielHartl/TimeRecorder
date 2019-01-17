using System;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Domain
{
    public interface IActivityService
    {
        Task ReportEventAsync(string tag, DateTimeOffset eventTime);

        Task<ActivitySummary> GetActivitySummaryAsync(string tag, TimeSpan timeZoneOffset);
    }
}