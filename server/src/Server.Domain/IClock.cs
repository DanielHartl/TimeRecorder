using System;

namespace TimeRecorder.Server.Domain
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}