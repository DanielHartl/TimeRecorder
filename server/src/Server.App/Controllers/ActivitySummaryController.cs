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
        public async Task<ActivitySummaryResponse> ActivitySummary(string key, TimeSpan timeZoneOffset)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var activitySummary = await _activityService.GetActivitySummaryAsync(key, timeZoneOffset);
            return _activitySummaryFormatter.ToResponse(activitySummary);
        }
    }
}