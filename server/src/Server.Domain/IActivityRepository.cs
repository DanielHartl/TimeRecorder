using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Domain
{
    public interface IActivityRepository
    {
        Task ReadAndCreateOrUpdateAsync(
            string key,
            DateTimeOffset timeKey,
            Func<TimeRangeCollection, TimeRangeCollection> readAndCreateOrUpdateOperation);

        Task<IEnumerable<TimeRangeCollection>> GetTimeRangesAsync(string key, DateTimeOffset from, DateTimeOffset to);
    }
}