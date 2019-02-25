using System;

namespace TimeRecorder.Server.Domain
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}