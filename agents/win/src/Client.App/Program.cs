using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActivityTracker.Client.App
{
    class Program
    {
        public static void Main(string[] args)
        {
            var endpoint = args[0];
            var tag = args[1];

            var errorReporter = new ErrorReporter();
            var eventReporter = new EventReporter(endpoint, exception => errorReporter.WriteWarning(exception.ToString()));
            var timeFilter = new TimeFilter(TimeSpan.FromSeconds(1));

            Action<Exception> exceptionHandler = exception => {
                errorReporter.WriteError(exception.ToString());
                Application.Exit();
            };

            Func<Task> reportEvent = async () => {
                await timeFilter.InvokeAsync(async () => {
                    await eventReporter.ReportActivityEventAsync(tag, DateTimeOffset.UtcNow);
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
