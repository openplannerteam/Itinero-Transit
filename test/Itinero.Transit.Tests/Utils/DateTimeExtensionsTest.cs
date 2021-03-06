using System;
using System.Linq;
using Itinero.Transit.Utils;
using Xunit;

namespace Itinero.Transit.Tests.Utils
{
    public class DateTimeExtensionsTest
    {
        [Fact]
        public void DateTimeConversion_GivenDate_ToAustralian()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");

            var dt = new DateTime(2020, 01, 20, 12, 0, 0, DateTimeKind.Utc);

            var australia = dt.ConvertTo(timeZone);
            Assert.Equal(new DateTime(2020, 01, 20, 23, 00, 00, DateTimeKind.Unspecified), australia);

            var localAgain = australia.ConvertToUtcFrom(timeZone);
            Assert.Equal(dt, localAgain);
        }

        [Fact]
        public void DateTimeConversion_GivenDate_ToBelgian()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels");

            var dt = new DateTime(2020, 01, 20, 12, 0, 0, DateTimeKind.Utc);

            var belgian = dt.ConvertTo(timeZone);
            Assert.Equal(new DateTime(2020, 01, 20, 13, 00, 00, DateTimeKind.Unspecified), belgian);

            var localAgain = belgian.ConvertToUtcFrom(timeZone);
            Assert.Equal(dt, localAgain);
        }

        [Fact]
        public void DateTimeConversion_GivenDate_ToBelizian()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Belize");

            var dt = new DateTime(2020, 01, 20, 12, 0, 0, DateTimeKind.Utc);

            var belizian = dt.ConvertTo(timeZone);
            Assert.Equal(new DateTime(2020, 01, 20, 6, 00, 00, DateTimeKind.Unspecified), belizian);

            var localAgain = belizian.ConvertToUtcFrom(timeZone);
            Assert.Equal(dt, localAgain);
        }

        [Fact]
        public void TimeZoneInfo_Count_MoreThen100()
        {
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            var c = tzs.Count;
            if (c < 100)
            {
                throw new Exception($"IF THIS TEST FAILS: make sure that the timezones are enabled on the linux machine you are using.\n" +
                                    $"Check that /usr/share/zoneinfo exists and is not empty. If it is, install locales or tzdata\n" +
                                    $"Are you working with docker images, such as teamcity-agents? Link the volumes /usr/share/zoneinfo together \n" +
                                    $"Only {c} timezones are found: " + string.Join(",",
                                        tzs.Select(tz => tz.Id)));
            }

            Assert.True(100 < c);
            Assert.NotNull(TimeZoneInfo.FindSystemTimeZoneById("Europe/Brussels"));
        }


    }
}