using System;
using System.Linq;
using Xunit;

namespace TimeRecorder.Server.Domain.Tests
{
    public class TimeRangeCollectionTests
    {
        [Fact]
        public void No_time_ranges_passed_results_in_empty_time_ranges()
        {
            var timeRangeCollection = new TimeRangeCollection(DateTimeOffset.Parse("2018/1/1"));
            Assert.Empty(timeRangeCollection.TimeRanges);
        }

        [Fact]
        public void Time_ranges_added_get_sorted()
        {
            var timeRangeCollection = new TimeRangeCollection(
                DateTimeOffset.Parse("2018/1/1"),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:30"), DateTimeOffset.Parse("2018/1/1 12:34")),
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:24"), DateTimeOffset.Parse("2018/1/1 12:26"))
                });

            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Equal(2, timeRanges.Length);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:24"), timeRanges[0].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:26"), timeRanges[0].End);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:30"), timeRanges[1].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:34"), timeRanges[1].End);
        }

        [Fact]
        public void Extends_time_range_when_adding_new_event()
        {
            var timeRangeCollection = new TimeRangeCollection(
                DateTimeOffset.Parse("2018-12-30T04:51:20+00:00").StartOfWeek(),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"))
                });

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018-12-30T04:51:21+00:00"), TimeSpan.FromMinutes(1));
            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Single(timeRanges);

            Assert.Equal(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), timeRanges[0].Start);
            Assert.Equal(DateTimeOffset.Parse("2018-12-30T04:51:21+00:00"), timeRanges[0].End);
        }

        [Fact]
        public void Extends_time_range_when_inserting_new_event_right_before_other_time_range()
        {
            var timeRangeCollection = new TimeRangeCollection(
                DateTimeOffset.Parse("2018-12-30T04:51:20.6243144+00:00").StartOfWeek(),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018-12-29T17:36:09+00:00"), DateTimeOffset.Parse("2018-12-29T17:36:40+00:00")),
                    new TimeRange(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"))
                });

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), TimeSpan.FromMinutes(1));
            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Equal(2, timeRanges.Length);

            Assert.Equal(DateTimeOffset.Parse("2018-12-29T17:36:09+00:00"), timeRanges[0].Start);
            Assert.Equal(DateTimeOffset.Parse("2018-12-29T17:36:40+00:00"), timeRanges[0].End);

            Assert.Equal(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), timeRanges[1].Start);
            Assert.Equal(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), timeRanges[1].End);
        }

        [Fact]
        public void Extends_time_range_when_inserting_new_event_right_after_other_time_range()
        {
            var timeRangeCollection = new TimeRangeCollection(
                DateTimeOffset.Parse("2018-12-30T04:51:20.6243144+00:00").StartOfWeek(),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018-12-29T17:36:09+00:00"), DateTimeOffset.Parse("2018-12-29T17:36:40+00:00")),
                    new TimeRange(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"))
                });

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018-12-29T17:36:41+00:00"), TimeSpan.FromMinutes(1));
            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Equal(2, timeRanges.Length);

            Assert.Equal(DateTimeOffset.Parse("2018-12-29T17:36:09+00:00"), timeRanges[0].Start);
            Assert.Equal(DateTimeOffset.Parse("2018-12-29T17:36:41+00:00"), timeRanges[0].End);

            Assert.Equal(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), timeRanges[1].Start);
            Assert.Equal(DateTimeOffset.Parse("2018-12-30T04:51:20+00:00"), timeRanges[1].End);
        }

        [Fact]
        public void Time_ranges_throws_if_ranges_overlap()
        {
            Assert.Throws<ArgumentException>(() => new TimeRangeCollection(
                DateTimeOffset.Parse("2018/1/1"),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:30"), DateTimeOffset.Parse("2018/1/1 12:34")),
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:24"), DateTimeOffset.Parse("2018/1/1 12:31"))
                }));
        }

        [Fact]
        public void Time_ranges_throws_if_range_not_in_correct_week()
        {
            Assert.Throws<ArgumentException>(() => new TimeRangeCollection(
                DateTimeOffset.Parse("2018/1/1"),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018/1/8 12:30"), DateTimeOffset.Parse("2018/1/8 12:34"))
                }));
        }

        [Fact]
        public void Time_ranges_throws_end_is_before_start()
        {
            Assert.Throws<ArgumentException>(() =>
                new TimeRange(DateTimeOffset.Parse("2018/1/1 12:36"), DateTimeOffset.Parse("2018/1/1 12:34")));
        }

        [Fact]
        public void Adding_events_creates_a_new_range_before_existing_ranges()
        {
            var timeRangeCollection = new TimeRangeCollection(
                DateTimeOffset.Parse("2018/1/1"),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:30"), DateTimeOffset.Parse("2018/1/1 12:34"))
                });

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:20"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:22"), TimeSpan.FromMinutes(5));

            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Equal(2, timeRanges.Length);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:20"), timeRanges[0].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:22"), timeRanges[0].End);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:30"), timeRanges[1].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:34"), timeRanges[1].End);
        }

        [Fact]
        public void Adding_events_creates_a_new_range_between_existing_ranges()
        {
            var timeRangeCollection = new TimeRangeCollection(
                DateTimeOffset.Parse("2018/1/1"),
                new[]
                {
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:30"), DateTimeOffset.Parse("2018/1/1 12:34")),
                    new TimeRange(DateTimeOffset.Parse("2018/1/1 12:10"), DateTimeOffset.Parse("2018/1/1 12:12"))
                });

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:20"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:22"), TimeSpan.FromMinutes(5));

            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Equal(3, timeRanges.Length);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:10"), timeRanges[0].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:12"), timeRanges[0].End);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:20"), timeRanges[1].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:22"), timeRanges[1].End);

            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:30"), timeRanges[2].Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:34"), timeRanges[2].End);
        }

        [Fact]
        public void Adding_event_creates_time_range()
        {
            var timeRangeCollection = new TimeRangeCollection(DateTimeOffset.Parse("2018/1/1"));

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:30"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:34"), TimeSpan.FromMinutes(5));

            var timeRange = timeRangeCollection.TimeRanges.SingleOrDefault();
            Assert.NotNull(timeRange);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:30"), timeRange.Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:34"), timeRange.End);
        }

        [Fact]
        public void Adding_event_ignored_if_within_time_range()
        {
            var timeRangeCollection = new TimeRangeCollection(DateTimeOffset.Parse("2018/1/1"));

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:30"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:34"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:32"), TimeSpan.FromMinutes(5));

            var timeRange = timeRangeCollection.TimeRanges.SingleOrDefault();
            Assert.NotNull(timeRange);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:30"), timeRange.Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:34"), timeRange.End);
        }

        [Fact]
        public void Adding_event_creates_new_time_range_if_not_within_tolerance_window()
        {
            var timeRangeCollection = new TimeRangeCollection(DateTimeOffset.Parse("2018/1/1"));

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:30"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:34"), TimeSpan.FromMinutes(5));

            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:40"), TimeSpan.FromMinutes(5));
            timeRangeCollection.AddEvent(DateTimeOffset.Parse("2018/1/1 12:42"), TimeSpan.FromMinutes(5));

            var timeRanges = timeRangeCollection.TimeRanges.ToArray();
            Assert.Equal(2, timeRanges.Length);

            var timeRange1 = timeRanges[0];
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:30"), timeRange1.Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:34"), timeRange1.End);

            var timeRange2 = timeRanges[1];
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:40"), timeRange2.Start);
            Assert.Equal(DateTimeOffset.Parse("2018/1/1 12:42"), timeRange2.End);
        }
    }
}
