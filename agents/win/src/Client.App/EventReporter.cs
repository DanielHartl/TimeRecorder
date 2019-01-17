using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ActivityTracker.Client.App
{
    internal class EventReporter
    {
        private readonly string _endpoint;
        private readonly Action<Exception> _errorHandler;

        public EventReporter(string endpoint, Action<Exception> errorHandler)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        public async Task ReportActivityEventAsync(string tag, DateTimeOffset eventTime)
        {
            try
            {
                var httpClient = new HttpClient { BaseAddress = new Uri(_endpoint) };

                var content = new StringContent(JsonConvert.SerializeObject(new {
                    tag,
                    eventTime
                }), Encoding.UTF8, "application/json");

                await httpClient.PostAsync("events", content);
            }
            catch (HttpRequestException httpRequestException)
            {
                _errorHandler(httpRequestException);
            }
        }
    }
}