using System;
using System.IO;

namespace TimeRecorder.Agent.App
{
    internal class Logger
    {
        private readonly string _logFile;

        public Logger(string logFile)
        {
            _logFile = logFile ?? throw new ArgumentNullException(nameof(logFile));
        }

        public void LogError(string message)
        {
            using (var streamWriter = new StreamWriter(_logFile, true))
            {
                streamWriter.WriteLine($"ERR  {DateTime.Now} {message}");
            }
        }

        public void LogWarning(string message)
        {
            using (var streamWriter = new StreamWriter(_logFile, true))
            {
                streamWriter.WriteLine($"WARN {DateTime.Now} {message}");
            }
        }

        public void LogInfo(string message)
        {
            using (var streamWriter = new StreamWriter(_logFile, true))
            {
                streamWriter.WriteLine($"INFO {DateTime.Now} {message}");
            }
        }
    }
}