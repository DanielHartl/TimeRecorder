using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeRecorder.Server.App.Contracts;
using TimeRecorder.Server.Domain;

namespace TimeRecorder.Server.App.Controllers
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
            await _activityService.ReportEventAsync(reportEventRequest.User, reportEventRequest.Events);
        }
    }
}
