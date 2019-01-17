using System;
using System.Threading.Tasks;
using ActivityTracker.Server.App.Contracts;
using ActivityTracker.Server.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ActivityTracker.Server.App.Controllers
{
    [Route("events")]
    public class EventApiController : Controller
    {
        private readonly IActivityService _activityService;

        public EventApiController(IActivityService activityService)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        }

        [HttpPost]
        public async Task ReportEventAsync([FromBody]ReportEventRequest reportEventRequest)
        {
            if (reportEventRequest == null) throw new ArgumentNullException(nameof(reportEventRequest));
            await _activityService.ReportEventAsync(reportEventRequest.Tag, reportEventRequest.EventTime);
        }
    }
}
