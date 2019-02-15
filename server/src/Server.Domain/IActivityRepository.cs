using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeRecorder.Server.Domain
{
    public interface IActivityRepository
    {
        Task ReadAndCreateOrUpdateAsync(
            string key,
            DateTimeOffset timeKey,
            Func<EventRecord, EventRecord> readAndCreateOrUpdateOperation);

        Task<IEnumerable<EventRecord>> GetEventRecords(string key, DateTimeOffset from, DateTimeOffset to);
    }
}