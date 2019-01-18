using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityTracker.Server.Domain;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace ActivityTracker.Server.Persistence.AzureStorage
{
    public class TableActivityRepository : IActivityRepository
    {
        private readonly CloudTableClient _tableClient;

        private const string TableName = "activities";

        private readonly HashSet<int> _retryableHttpStatusCodes = new HashSet<int>(new[] { 409, 412 });

        public TableActivityRepository(StorageCredentials storageCredentials)
        {
            _tableClient = new CloudStorageAccount(storageCredentials, true).CreateCloudTableClient();
        }

        public async Task ReadAndCreateOrUpdateAsync(
            string tag,
            DateTimeOffset timeKey,
            Func<TimeRangeCollection, TimeRangeCollection> readAndCreateOrUpdateOperation)
        {
            var tableReference = _tableClient.GetTableReference(TableName);
            await tableReference.CreateIfNotExistsAsync();

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tag),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, GetRowKey(timeKey)));

            while (true)
            {
                var tableQuery = new TableQuery<TimeRangeCollectionEntity>().Where(filter).Take(1);
                var readResult =
                    await tableReference.ExecuteQuerySegmentedAsync(tableQuery, new TableContinuationToken());

                var activityEntity = readResult.Results.FirstOrDefault();
                var etag = activityEntity?.ETag;

                var result = readAndCreateOrUpdateOperation(ToDomainObject(activityEntity));
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

        public async Task<IEnumerable<TimeRangeCollection>> GetTimeRangesAsync(
            string key,
            DateTimeOffset from,
            DateTimeOffset to)
        {
            //if (from != from.StartOfWeek()) throw new ArgumentException(nameof(from));
            //if (to != to.StartOfWeek()) throw new ArgumentException(nameof(to));

            var tableReference = _tableClient.GetTableReference(TableName);
            await tableReference.CreateIfNotExistsAsync();

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, GetRowKey(from)),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, GetRowKey(to))));

            var tableQuery = new TableQuery<TimeRangeCollectionEntity>().Where(filter);
            var tableContinuationToken = new TableContinuationToken();

            var results = new List<TimeRangeCollection>();

            while (tableContinuationToken != null)
            {
                var readResult = await tableReference.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken);
                results.AddRange(readResult.Results.Select(ToDomainObject));
                tableContinuationToken = readResult.ContinuationToken;
            }

            return results;
        }

        private static string GetRowKey(TimeRangeCollection timeRangeCollection) => GetRowKey(timeRangeCollection.StartDate);
        private static string GetRowKey(DateTimeOffset timeKey) => timeKey.ToString("yyyyMMdd");

        private static TimeRangeCollection ToDomainObject(TimeRangeCollectionEntity timeRangeCollectionEntity)
        {
            if (timeRangeCollectionEntity == null) return null;

            var timeRangeEntities = JsonConvert.DeserializeObject<TimeRangeEntity[]>(timeRangeCollectionEntity.TimeRangesJson);
            var timeRanges = timeRangeEntities.Select(x => new TimeRange(new DateTimeOffset(x.StartTime, TimeSpan.Zero), new DateTimeOffset(x.EndTime, TimeSpan.Zero)));
            return new TimeRangeCollection(timeRangeCollectionEntity.StartDate, timeRanges);
        }

        private static TimeRangeCollectionEntity ToEntity(TimeRangeCollection timeRangeCollection, string partitionKey, string etag)
        {
            var rowKey = GetRowKey(timeRangeCollection);
            var timeRangeEntities = timeRangeCollection.TimeRanges.Select(x => new TimeRangeEntity {
                StartTime = x.Start.UtcDateTime,
                EndTime = x.End.UtcDateTime
            });

            var timeRangesAsJson = JsonConvert.SerializeObject(timeRangeEntities);

            return new TimeRangeCollectionEntity {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                StartDate = timeRangeCollection.StartDate,
                TimeRangesJson = timeRangesAsJson,
                ETag = etag
            };
        }
    }
}