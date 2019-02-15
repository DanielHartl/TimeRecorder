using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TimeRecorder.Agent.App
{
    internal static class NativeFunctions
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dwTime;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                while (true)
                {
                    Console.WriteLine($"Running on linux {DateTime.Now}");
                    Thread.Sleep(10000);
                }
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

            while (true)
            {
                var time = GetLastInputTime();

                if (time < 2)
                {
                    Console.WriteLine($"REPORTING EVENT for {user} to {endpoint}");
                    eventAggregator.AddEvent(DateTimeOffset.UtcNow);
                }

                Thread.Sleep(1000);
            }
        }

        static uint GetLastInputTime()
        {
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if (NativeFunctions.GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }
    }
}
