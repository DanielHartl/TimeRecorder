using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            var start = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
            var end = start.AddMinutes(2);

            var eventReporter = new EventReporter(ServerEndpoint, ex => Debug.WriteLine(ex.ToString()));
            await eventReporter.ReportActivityEventAsync(TestUser, start);
            await eventReporter.ReportActivityEventAsync(TestUser, end);

            var httpClient = new HttpClient();
            var resultJson = await httpClient.GetStringAsync($"{ServerEndpoint}/api/activitysummary?user={TestUser}&timeZoneOffset=0");

            var activitySummaryReponse = JsonConvert.DeserializeObject<ActivitySummaryResponse>(resultJson);
            Assert.Equal("00:02", activitySummaryReponse.WeeklySummaryModels.First().TotalDuration);
        }
    }
}
