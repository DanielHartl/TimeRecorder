using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeRecorder.Agent.App
{
    public class EventReporter
    {
        private readonly Uri _endpoint;
        private readonly Action<Exception> _warningHandler;
        private readonly Action<Exception> _errorHandler;

        public EventReporter(
            Uri endpoint,
            Action<Exception> warningHandler,
            Action<Exception> errorHandler)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _warningHandler = warningHandler ?? throw new ArgumentNullException(nameof(warningHandler));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        public async Task ReportActivityEventAsync(string user, IDictionary<DateTimeOffset, int> events, TimeSpan timeout)
        {
            await RetryAsync(async () =>
            {
                var httpClient = new HttpClient { BaseAddress = _endpoint };

                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    user,
                    events
                }), Encoding.UTF8, "application/json");

                await httpClient.PostAsync("events", content);
            }, timeout);
        }

        private async Task RetryAsync(Func<Task> operation, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception exception) when (IsTransientException(exception))
                {
                    await Task.Delay(1000);

                    if (sw.Elapsed > timeout)
                    {
                        _errorHandler(exception);
                        return;
                    }

                    _warningHandler(exception);
                }
            }
        }

        private static bool IsTransientException(Exception exception) => exception is TaskCanceledException || exception is HttpRequestException;
    }
}