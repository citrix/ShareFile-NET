using System;

using FluentAssertions;

using NUnit.Framework;

using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests.Filters;

namespace ShareFile.Api.Client.Tests.Requests.Filters
{
    [TestFixture]
    public class DateFilterTests
    {
        [TestCase]
        public void DateGreaterThanFilter()
        {
            var dt = LocalDate();
            var filter = new GreaterThanFilter("creationdate", Filter.Function.Date(dt));

            string expected = string.Format("creationdate gt date({0})", ExpectedSerialization(dt));
            filter.ToString().Should().Be(expected);
        }

        [TestCase]
        public void DateLessThanFilter()
        {
            var dt = LocalDate();
            var filter = new LessThanFilter("creationdate", Filter.Function.Date(dt));

            string expected = string.Format("creationdate lt date({0})", ExpectedSerialization(dt));
            filter.ToString().Should().Be(expected);
        }

        [TestCase]
        public void TimeGreaterThanFilter()
        {
            var dt = LocalDate();
            var filter = new GreaterThanFilter("creationdate", Filter.Function.Time(dt));

            string expected = string.Format("creationdate gt time({0})", ExpectedSerialization(dt));
            filter.ToString().Should().Be(expected);
        }

        [TestCase]
        public void TimeLessThanFilter()
        {
            var dt = LocalDate();
            var filter = new LessThanFilter("creationdate", Filter.Function.Time(dt));

            string expected = string.Format("creationdate lt time({0})", ExpectedSerialization(dt));
            filter.ToString().Should().Be(expected);
        }

        private DateTime LocalDate()
        {
            return new DateTime(2016, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
        }

        private string ExpectedSerialization(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("u");
        }
    }
}
