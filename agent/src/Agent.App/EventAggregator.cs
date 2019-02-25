using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimeRecorder.Agent.App
{
    internal class EventAggregator
    {
        private readonly Func<IDictionary<DateTimeOffset, int>, Task> _reportEventsFunc;
        private readonly TimeSpan _groupByPrecision;
		private readonly TimeSpan _reportingInterval;
        private readonly Action<Exception> _exceptionHandler;
        private readonly CancellationToken _cancellationToken;
        private readonly ConcurrentQueue<DateTimeOffset> _timeStamps = new ConcurrentQueue<DateTimeOffset>();

        public EventAggregator(
            Func<IDictionary<DateTimeOffset, int>, Task> reportEventsFunc,
            TimeSpan groupByPrecision,
            TimeSpan reportingInterval,
            Action<Exception> exceptionHandler,
            CancellationToken cancellationToken)
        {
            _reportEventsFunc = reportEventsFunc ?? throw new ArgumentNullException(nameof(reportEventsFunc));
            _groupByPrecision = groupByPrecision;
            _reportingInterval = reportingInterval;
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _cancellationToken = cancellationToken;

            Run();
        }

        public void AddEvent(DateTimeOffset timestamp)
        {
            _timeStamps.Enqueue(timestamp);
        }

        private void Run()
        {
            Task.Run(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await FlushAsync();
                        await Task.Delay(_reportingInterval, _cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (Exception exception)
                    {
                        _exceptionHandler(exception);
                    }
                }
            },
            _cancellationToken);
        }

        private IEnumerable<DateTimeOffset> DequeueTimestamps()
        {
            while (_timeStamps.TryDequeue(out var item))
            {
                yield return item;
            }
        }

        private async Task FlushAsync()
        {
            var events = DequeueTimestamps()
                .GroupBy(t => Floor(t, _groupByPrecision))
                .ToDictionary(t => t.Key, t => t.Count());

            if (events.Count > 0)
            {
                await _reportEventsFunc(events);
            }
        }

        private static DateTimeOffset Floor(DateTimeOffset dateTimeOffset, TimeSpan precision)
        {
            return new DateTimeOffset(
                dateTimeOffset.Ticks - dateTimeOffset.Ticks % precision.Ticks,
                dateTimeOffset.Offset);
        }
    }
}