using ActivityTracker.Server.App.Contracts;
using ActivityTracker.Server.App.Formatter;
using ActivityTracker.Server.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ActivityTracker.Server.App.Controllers
{
    [Route("api/[controller]")]
    public class ActivitySummaryController : Controller
    {
        private readonly IActivityService _activityService;

        private readonly IActivitySummaryFormatter _activitySummaryFormatter;

        public ActivitySummaryController(IActivityService activityService, IActivitySummaryFormatter activitySummaryFormatter)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            _activitySummaryFormatter = activitySummaryFormatter ?? throw new ArgumentNullException(nameof(activitySummaryFormatter));
        }

        [HttpGet]
        public async Task<ActivitySummaryResponse> ActivitySummary(string user, [FromQuery(Name = "timeZoneOffset")] int? timeZoneOffsetInMinutes)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var activitySummary = await _activityService.GetActivitySummaryAsync(user, GetTimeZoneOffset(timeZoneOffsetInMinutes));
            return _activitySummaryFormatter.ToResponse(activitySummary);
        }

        private static TimeSpan GetTimeZoneOffset(int? timeZoneOffsetInMinutes)
        {
            return timeZoneOffsetInMinutes.HasValue
                ? TimeSpan.FromMinutes(-timeZoneOffsetInMinutes.Value)
                : TimeSpan.Zero;
        }
    }
}