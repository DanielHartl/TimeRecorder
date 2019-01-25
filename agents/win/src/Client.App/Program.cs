using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActivityTracker.Client.App
{
    class Program
    {
        public static void Main(string[] args)
        {
            var user = args[0];
            var endpoint = args[1];

            var timeFilterInterval = args.Length > 2 ? int.Parse(args[2]) : 10;

            var logFolder = Path.Combine(Path.GetTempPath(), "logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logFile = Path.Combine(logFolder, "log.txt");
            Console.WriteLine(logFile);

            var errorReporter = new ErrorReporter(logFile);
            var eventReporter = new EventReporter(endpoint, exception => errorReporter.WriteWarning(exception.ToString()));
            var timeFilter = new TimeFilter(TimeSpan.FromSeconds(timeFilterInterval));

            Action<Exception> exceptionHandler = exception => {
                errorReporter.WriteError(exception.ToString());
                Application.Exit();
            };

            Func<Task> reportEvent = async () => {
                await timeFilter.InvokeAsync(async () => {
                    await eventReporter.ReportActivityEventAsync(user, DateTimeOffset.UtcNow);
                });
            };

            using (new KeyboardIntercepter(
                async key => await reportEvent(),
                exceptionHandler))
            {
                using (new MouseIntercepter(
                    async mouseEvent => await reportEvent(),
                    exceptionHandler,
                    MouseEventType.LeftButtonUp,
                    MouseEventType.RightButtonUp))
                {
                    Application.Run();
                }
            }
        }
    }
}
