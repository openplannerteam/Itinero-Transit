using System;
using Itinero.Transit.Data.Walks;
using Xunit;

namespace Itinero.Transit.Tests.utils
{
    public class DateTrackerTest
    {
        [Fact]
        public void TestTimeAggregation()
        {
            var dt = new DateTracker();

            Assert.Empty(dt.TimeWindows());

            var d = new DateTime(2019, 01, 28, 10, 00, 00);

            dt.AddTimeWindow(d, d.AddMinutes(30));
            Assert.Single(dt.TimeWindows());
            Assert.Equal((d, d.AddMinutes(30)), dt.TimeWindows()[0]);


            dt.AddTimeWindow(d.AddMinutes(15), d.AddMinutes(45));


            Assert.Single(dt.TimeWindows());
            Assert.Equal((d, d.AddMinutes(45)), dt.TimeWindows()[0]);
            dt.AddTimeWindow(d.AddMinutes(-15), d.AddMinutes(45));
            Assert.Single(dt.TimeWindows());
            Assert.Equal((d.AddMinutes(-15), d.AddMinutes(45)), dt.TimeWindows()[0]);


            dt.AddTimeWindow(d.AddMinutes(-60), d.AddMinutes(-30));
            Assert.Equal(2, dt.TimeWindows().Count);


            dt.AddTimeWindow(d.AddMinutes(-30), d.AddMinutes(0));
            Assert.Single(dt.TimeWindows());
            Assert.Equal((d.AddMinutes(-60), d.AddMinutes(45)), dt.TimeWindows()[0]);
        }

        [Fact]
        public void TestGaps()
        {
            var dt = new DateTracker();
            var d = new DateTime(2019, 01, 28, 10, 00, 00);

            dt.AddTimeWindow(d, d.AddMinutes(30));

            var gaps = dt.Gaps(d.AddMinutes(-15), d.AddMinutes(45));
            Assert.Equal(2, gaps.Count);
            Assert.Equal((d.AddMinutes(-15), d), gaps[0]);
            Assert.Equal((d.AddMinutes(30), d.AddMinutes(45)), gaps[1]);

            dt.AddTimeWindow(d.AddMinutes(15), d.AddMinutes(45));

            gaps = dt.Gaps(d.AddMinutes(-15), d.AddMinutes(45));
            Assert.Single(gaps);
            Assert.Equal((d.AddMinutes(-15), d), gaps[0]);
        }
    }
}