using System;

namespace ActivityTracker.Server.Domain
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}