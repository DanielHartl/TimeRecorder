using System.IO;

namespace ActivityTracker.Client.App
{
    internal class ErrorReporter
    {
        private readonly string _logs;

        public ErrorReporter()
        {
            _logs = Path.Combine(Path.GetTempPath(), "activity.logs");
        }

        public void WriteError(string message)
        {
            using (var streamWriter = new StreamWriter(_logs, true))
            {
                streamWriter.WriteLine("ERR  " + message);
            }
        }

        public void WriteWarning(string message)
        {
            using (var streamWriter = new StreamWriter(_logs, true))
            {
                streamWriter.WriteLine("WARN " + message);
            }
        }
    }
}