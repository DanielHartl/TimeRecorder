using System;

namespace ActivityTracker.Server.Domain
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}