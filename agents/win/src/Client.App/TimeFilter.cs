using System;
using System.Threading.Tasks;

namespace ActivityTracker.Client.App
{
    class TimeFilter
    {
        private readonly TimeSpan _filterTimeSpan;
        private readonly Func<DateTime> _utcNowFunc;

        private DateTime _nextUpdate = DateTime.MinValue;

        public TimeFilter(TimeSpan filterTimeSpan, Func<DateTime> utcNowFunc = null)
        {
            _filterTimeSpan = filterTimeSpan;
            _utcNowFunc = utcNowFunc ?? (() => DateTime.UtcNow);
        }

        public async Task InvokeAsync(Func<Task> action)
        {
            if (_nextUpdate == DateTime.MinValue || _nextUpdate < _utcNowFunc())
            {
                await action();
                _nextUpdate = _utcNowFunc() + _filterTimeSpan;
            }
        }
    }
}