using ActivityTracker.Client.App;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ActivityTracker.Agent.App
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var user = args[0];
            var endpoint = new Uri(args[1]);

            //var timeFilterInterval = args.Length > 2 ? int.Parse(args[2]) : 10;
            var reportingTimeout = TimeSpan.FromMinutes(15);

            var cancellationTokenSource = new CancellationTokenSource();

            var logFolder = Path.Combine(Path.GetTempPath(), "logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logFile = Path.Combine(logFolder, "log.txt");

            var logger = new Logger(logFile);
            var eventReporter = new EventReporter(
                endpoint,
                exception => logger.LogWarning(exception.ToString()),
                exception => logger.LogError(exception.ToString()));

            var eventAggregator = new EventAggregator(
                async events =>
                {
                    logger.LogInfo($"{user} reports {events.Values.Sum()} events");
                    await eventReporter.ReportActivityEventAsync(user, events, reportingTimeout);
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(30),
                exception => logger.LogError(exception.ToString()),
                cancellationTokenSource.Token);

            Application.ApplicationExit += (s, e) =>
            {
                cancellationTokenSource.Cancel();
            };

            Action<Exception> exceptionHandler = exception =>
            {
                logger.LogError(exception.ToString());
                Application.Exit();
            };

            using (new KeyboardIntercepter(
                key => eventAggregator.AddEvent(DateTimeOffset.UtcNow),
                exceptionHandler))
            {
                using (new MouseIntercepter(
                    mouseEvent => eventAggregator.AddEvent(DateTimeOffset.UtcNow),
                    exceptionHandler,
                    MouseEventType.LeftButtonUp,
                    MouseEventType.RightButtonUp,
                    MouseEventType.MouseWheel))
                {
                    Application.Run();
                }
            }
        }
    }
}
