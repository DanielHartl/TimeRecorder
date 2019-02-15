using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeRecorder.Server.App.Contracts;
using TimeRecorder.Server.App.Formatter;
using TimeRecorder.Server.Domain;

namespace TimeRecorder.Server.App.Controllers
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