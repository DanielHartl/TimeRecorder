using System;

namespace TimeRecorder.Agent.App
{
    public class LinuxInputListener : IInputListener
    {
        public TimeSpan GetTimeSinceLastInput()
        {
            return TimeSpan.MaxValue;
        }
    }
}