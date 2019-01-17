using ActivityTracker.Server.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Persistence
{
    public class InMemoryActivityRepository : IActivityRepository
    {
        private readonly ConcurrentDictionary<(string tag, DateTimeOffset timeKey), TimeRangeCollection> _dataStore = new ConcurrentDictionary<(string tag, DateTimeOffset timeKey), TimeRangeCollection>();

        public async Task ReadAndCreateOrUpdateAsync(string tag, DateTimeOffset timeKey, Func<TimeRangeCollection, TimeRangeCollection> readAndCreateOrUpdateOperation)
        {
            var key = (tag, timeKey);

            await Task.CompletedTask;
            _dataStore.TryGetValue(key, out var result);
            var updatedData = readAndCreateOrUpdateOperation(result);
            _dataStore.AddOrUpdate(key, updatedData, (k, v) => updatedData);
        }

        public async Task<IEnumerable<TimeRangeCollection>> GetTimeRangesAsync(string tag, DateTimeOffset from, DateTimeOffset to)
        {
            await Task.CompletedTask;
            return _dataStore.Where(x => x.Key.tag == tag && x.Key.timeKey >= from && x.Key.timeKey < to).Select(x => x.Value);
        }
    }
}
