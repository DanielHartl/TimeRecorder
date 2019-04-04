using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TimeRecorder.Agent.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            IInputListener inputListener;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                inputListener = new WindowsInputListener();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                inputListener = new LinuxInputListener();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            var user = args[0];
            var endpoint = new Uri(args[1]);

            var reportingTimeout = TimeSpan.FromMinutes(15);
            var cancellationTokenSource = new CancellationTokenSource();

            var logFolder = Path.Combine(Path.GetTempPath(), "logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logFile = Path.Combine(logFolder, "log.txt");
            Console.WriteLine(logFile);

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

            try
            {
                while (true)
                {
                    var time = inputListener.GetTimeSinceLastInput();

                    if (time < TimeSpan.FromSeconds(2))
                    {
                        logger.LogInfo($"REPORTING EVENT for {user} to {endpoint}");
                        eventAggregator.AddEvent(DateTimeOffset.UtcNow);
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception.ToString());
            }
        }
    }
}
