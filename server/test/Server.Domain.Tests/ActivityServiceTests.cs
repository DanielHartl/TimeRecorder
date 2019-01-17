using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ActivityTracker.Server.Domain.Tests
{
    public class ActivityServiceTests
    {
        [Fact]
        public async Task ParseTimeRanges()
        {
            const string tag = "testtag";

            var clockMock = new Mock<IClock>();
            clockMock.Setup(x => x.UtcNow).Returns(DateTime.Parse("2008/1/27 12:01"));

            var activityRepositoryMock = new Mock<IActivityRepository>();
            activityRepositoryMock.Setup(x => x.GetTimeRangesAsync(
                tag,
                DateTimeOffset.Parse("2008/1/5 00:00"),
                DateTimeOffset.Parse("2008/1/28 00:00")))
                .ReturnsAsync(() => new[]
                {
                    new TimeRangeCollection(DateTimeOffset.Parse("2008/1/7 00:00 +00:00"),
                        new[]
                        {
                            new TimeRange(DateTimeOffset.Parse("2008/1/7 01:01 +00:00"), DateTimeOffset.Parse("2008/1/7 01:10 +00:00")),
                            new TimeRange(DateTimeOffset.Parse("2008/1/7 02:01 +00:00"), DateTimeOffset.Parse("2008/1/7 02:10 +00:00"))
                        }),
                    new TimeRangeCollection(DateTimeOffset.Parse("2008/1/8 00:00 +00:00"),
                        new[]
                        {
                            new TimeRange(DateTimeOffset.Parse("2008/1/8 01:01 +00:00"), DateTimeOffset.Parse("2008/1/8 01:10 +00:00"))
                        }),
                    new TimeRangeCollection(DateTimeOffset.Parse("2008/1/25 00:00 +00:00"),
                        new[]
                        {
                            new TimeRange(DateTimeOffset.Parse("2008/1/25 01:01 +00:00"), DateTimeOffset.Parse("2008/1/25 01:11 +00:00")),
                            new TimeRange(DateTimeOffset.Parse("2008/1/25 02:01 +00:00"), DateTimeOffset.Parse("2008/1/25 02:11 +00:00"))
                        })
                });

            var activityService = new ActivityService(
                activityRepositoryMock.Object,
                TimeSpan.FromSeconds(1),
                clockMock.Object);

            var activitySummary = await activityService.GetActivitySummaryAsync(tag, TimeSpan.FromHours(-1));

            var weeklySummaries = activitySummary.WeeklySummaries.ToArray();
            Assert.Equal(2, weeklySummaries.Length);

            var dailySummaries1 = weeklySummaries[0].DailySummaries.ToArray();
            Assert.Single(dailySummaries1);

            var timeRanges11 = dailySummaries1[0].TimeRanges.ToArray();
            Assert.Equal(2, timeRanges11.Length);

            Assert.Equal(DateTime.Parse("2008/1/25 12:01 AM"), timeRanges11[0].Start);
            Assert.Equal(DateTime.Parse("2008/1/25 12:11 AM"), timeRanges11[0].End);

            Assert.Equal(DateTime.Parse("2008/1/25 1:01 AM"), timeRanges11[1].Start);
            Assert.Equal(DateTime.Parse("2008/1/25 1:11 AM"), timeRanges11[1].End);

            var dailySummaries2 = weeklySummaries[1].DailySummaries.ToArray();

            var timeRanges21 = dailySummaries2[0].TimeRanges.ToArray();
            Assert.Single(timeRanges21);

            Assert.Equal(DateTime.Parse("2008/1/8 12:01 AM"), timeRanges21[0].Start);
            Assert.Equal(DateTime.Parse("2008/1/8 12:10 AM"), timeRanges21[0].End);
            
            var timeRanges22 = dailySummaries2[1].TimeRanges.ToArray();
            Assert.Equal(2, timeRanges22.Length);

            Assert.Equal(DateTime.Parse("2008/1/7 12:01 AM"), timeRanges22[0].Start);
            Assert.Equal(DateTime.Parse("2008/1/7 12:10 AM"), timeRanges22[0].End);

            Assert.Equal(DateTime.Parse("2008/1/7 1:01 AM"), timeRanges22[1].Start);
            Assert.Equal(DateTime.Parse("2008/1/7 1:10 AM"), timeRanges22[1].End);
        }
    }
}