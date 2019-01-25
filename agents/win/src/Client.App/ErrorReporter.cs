using System;
using System.IO;

namespace ActivityTracker.Client.App
{
    internal class ErrorReporter
    {
        private readonly string _logFile;

        public ErrorReporter(string logFile)
        {
            _logFile = logFile ?? throw new ArgumentNullException(nameof(logFile));
        }

        public void WriteError(string message)
        {
            using (var streamWriter = new StreamWriter(_logFile, true))
            {
                streamWriter.WriteLine("ERR  " + message);
            }
        }

        public void WriteWarning(string message)
        {
            using (var streamWriter = new StreamWriter(_logFile, true))
            {
                streamWriter.WriteLine("WARN " + message);
            }
        }
    }
}