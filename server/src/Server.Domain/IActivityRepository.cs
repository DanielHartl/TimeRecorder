using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityTracker.Server.Domain
{
    public interface IActivityRepository
    {
        Task ReadAndCreateOrUpdateAsync(
            string tag,
            DateTimeOffset timeKey,
            Func<TimeRangeCollection, TimeRangeCollection> readAndCreateOrUpdateOperation);

        Task<IEnumerable<TimeRangeCollection>> GetTimeRangesAsync(string tag, DateTimeOffset from, DateTimeOffset to);
    }
}