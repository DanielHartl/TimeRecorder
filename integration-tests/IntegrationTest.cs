using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TimeRecorder.Agent.App;
using TimeRecorder.Server.App.Contracts;
using Xunit;

namespace TimeRecorder.IntegrationTests
{
    public class IntegrationTest
    {
        private const string ServerEndpoint = "http://localhost:80";
        private const string TestUser = "user1";

        [Fact]
        public async Task When_event_gets_sent_summary_shows()
        {
            var startTime = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);

            var start = new Dictionary<DateTimeOffset, int> { { startTime, 1 } };
            var end = new Dictionary<DateTimeOffset, int> { { startTime.AddMinutes(2), 1 } };

            var eventReporter = new EventReporter(
                new Uri(ServerEndpoint),
                ex => Debug.WriteLine(ex.ToString()),
                ex => Debug.WriteLine(ex.ToString()));

            await eventReporter.ReportActivityEventAsync(TestUser, start, TimeSpan.FromSeconds(20));
            await eventReporter.ReportActivityEventAsync(TestUser, end, TimeSpan.FromSeconds(20));

            var httpClient = new HttpClient();
            var resultJson = await httpClient.GetStringAsync($"{ServerEndpoint}/api/activitysummary?user={TestUser}&timeZoneOffset=0");

            var activitySummaryResponse = JsonConvert.DeserializeObject<ActivitySummaryResponse>(resultJson);
            Assert.Equal("00:02", activitySummaryResponse.DailySummaryModels.First().TotalDuration);
        }
    }
}
