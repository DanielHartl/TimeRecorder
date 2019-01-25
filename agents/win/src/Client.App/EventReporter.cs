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

        public async Task ReportActivityEventAsync(string user, DateTimeOffset eventTime)
        {
            await RetryAsync(async () => {
                var httpClient = new HttpClient { BaseAddress = new Uri(_endpoint) };

                var content = new StringContent(JsonConvert.SerializeObject(new {
                    user,
                    eventTime
                }), Encoding.UTF8, "application/json");

                await httpClient.PostAsync("events", content);
            }, 10);
        }

        private async Task RetryAsync(Func<Task> operation, int times)
        {
            for (var i = 0; i < times; i++)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception exception) when (IsTransientException(exception))
                {
                    _errorHandler(exception);
                }
            }
        }

        private static bool IsTransientException(Exception exception) => exception is TaskCanceledException || exception is HttpRequestException;
    }
}