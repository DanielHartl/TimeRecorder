using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using TimeRecorder.Server.Domain;

namespace TimeRecorder.Server.Persistence.AzureStorage
{
    public class TableActivityRepository : IActivityRepository
    {
        private readonly CloudTableClient _tableClient;

        private const string TableName = "events";

        private readonly HashSet<int> _retryableHttpStatusCodes = new HashSet<int>(new[] { 409, 412 });

        public TableActivityRepository(StorageCredentials storageCredentials)
        {
            _tableClient = new CloudStorageAccount(storageCredentials, true).CreateCloudTableClient();
        }

        public async Task ReadAndCreateOrUpdateAsync(
            string tag,
            DateTimeOffset timeKey,
            Func<EventRecord, EventRecord> readAndCreateOrUpdateOperation)
        {
            var tableReference = _tableClient.GetTableReference(TableName);
            await tableReference.CreateIfNotExistsAsync();

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tag),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, GetRowKey(timeKey)));

            while (true)
            {
                var tableQuery = new TableQuery<EventRecordEntity>().Where(filter).Take(1);
                var readResult =
                    await tableReference.ExecuteQuerySegmentedAsync(tableQuery, new TableContinuationToken());

                var eventRecordEntity = readResult.Results.FirstOrDefault();
                var etag = eventRecordEntity?.ETag;

                var result = readAndCreateOrUpdateOperation(ToDomainObject(eventRecordEntity));
                var resultEntity = ToEntity(result, tag, etag);

                try
                {
                    var operation = etag == null
                        ? TableOperation.Insert(resultEntity)
                        : TableOperation.Merge(resultEntity);

                    await tableReference.ExecuteAsync(operation);

                    return;
                }
                catch (StorageException storageException)
                {
                    if (_retryableHttpStatusCodes.Contains(storageException.RequestInformation.HttpStatusCode))
                    {
                        continue;
                    }

                    throw;
                }
            }
        }

        public async Task<IEnumerable<EventRecord>> GetEventRecords(
            string key,
            DateTimeOffset from,
            DateTimeOffset to)
        {
            var tableReference = _tableClient.GetTableReference(TableName);
            await tableReference.CreateIfNotExistsAsync();

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, GetRowKey(from)),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, GetRowKey(to))));

            var tableQuery = new TableQuery<EventRecordEntity>().Where(filter);
            var tableContinuationToken = new TableContinuationToken();

            var results = new List<EventRecord>();

            while (tableContinuationToken != null)
            {
                var readResult = await tableReference.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken);
                results.AddRange(readResult.Results.Select(ToDomainObject));
                tableContinuationToken = readResult.ContinuationToken;
            }

            return results;
        }

        private static string GetRowKey(EventRecord eventRecord) => GetRowKey(eventRecord.BaseTime);
        private static string GetRowKey(DateTimeOffset timeKey) => timeKey.UtcDateTime.ToString("yyyyMMddHHmm");

        private static EventRecord ToDomainObject(EventRecordEntity eventRecordEntity)
        {
            if (eventRecordEntity == null) return null;

            var minuteEntries = JsonConvert.DeserializeObject<EventRecordMinuteEntry[]>(eventRecordEntity.Payload);

            var eventRecord = new EventRecord(new DateTimeOffset(eventRecordEntity.BaseDate, TimeSpan.Zero));
            eventRecord.AddEvents(minuteEntries.Select(x =>
                new KeyValuePair<DateTimeOffset, int>(
                    eventRecord.BaseTime.AddMinutes(x.Minute),
                    x.Count)));

            return eventRecord;
        }

        private static EventRecordEntity ToEntity(EventRecord eventRecord, string partitionKey, string etag)
        {
            var rowKey = GetRowKey(eventRecord);
            var minuteEntries = eventRecord
                .Events
                .Select(x => new EventRecordMinuteEntry
                {
                    Minute = (int)(x.Key - eventRecord.BaseTime).TotalMinutes,
                    Count = x.Value
                })
                .OrderBy(x => x.Minute);

            var minuteEntriesAsJson = JsonConvert.SerializeObject(minuteEntries);

            return new EventRecordEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                BaseDate = eventRecord.BaseTime.UtcDateTime,
                Payload = minuteEntriesAsJson,
                ETag = etag
            };
        }
    }
}