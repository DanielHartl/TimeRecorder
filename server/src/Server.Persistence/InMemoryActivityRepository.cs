using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeRecorder.Server.Domain;

namespace TimeRecorder.Server.Persistence
{
    public class InMemoryActivityRepository : IActivityRepository
    {
        private readonly ConcurrentDictionary<(string tag, DateTimeOffset timeKey), EventRecord> _dataStore = new ConcurrentDictionary<(string tag, DateTimeOffset timeKey), EventRecord>();

        //public InMemoryActivityRepository()
        //: this(null, DateTimeOffset.MinValue, null)
        //{
        //}

        //public InMemoryActivityRepository(string tag, DateTimeOffset start, string timeRangesJson)
        //{
        //    //if (timeRangesJson != null)
        //    //{
        //    //    var obj = JsonConvert.DeserializeAnonymousType(timeRangesJson,
        //    //        new[]
        //    //        {
        //    //            new
        //    //            {
        //    //                s = DateTime.MinValue,
        //    //                e = DateTime.MinValue
        //    //            }
        //    //        }
        //    //    );

        //    //    _dataStore.TryAdd((tag, timeKey: start),
        //    //        new TimeRangeCollection(start,
        //    //            obj.Select(x => new TimeRange(new DateTimeOffset(x.s), new DateTimeOffset(x.e)))));
        //    //}
        //}

        public async Task ReadAndCreateOrUpdateAsync(string tag, DateTimeOffset timeKey, Func<EventRecord, EventRecord> readAndCreateOrUpdateOperation)
        {
            var key = (tag, timeKey);

            await Task.CompletedTask;
            _dataStore.TryGetValue(key, out var result);
            var updatedData = readAndCreateOrUpdateOperation(result);
            _dataStore.AddOrUpdate(key, updatedData, (k, v) => updatedData);
        }

        public async Task<IEnumerable<EventRecord>> GetEventRecords(string key, DateTimeOffset from, DateTimeOffset to)
        {
            await Task.CompletedTask;
            return _dataStore.Where(x => x.Key.tag == key && x.Key.timeKey >= from && x.Key.timeKey < to).Select(x => x.Value);
        }
    }
}
