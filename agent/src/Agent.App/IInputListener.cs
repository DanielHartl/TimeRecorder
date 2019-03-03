using System;

namespace TimeRecorder.Agent.App
{
    public interface IInputListener
    {
        TimeSpan GetTimeSinceLastInput();
    }
}