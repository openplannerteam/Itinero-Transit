using System;
using Itinero.Transit.Data;
using Xunit;

namespace Itinero.Transit.Tests.Functional.IO.OSM
{
    public class OpeningHoursTest
    {
        [Fact]
        public void TestOh()
        {
            var r247 = TwentyFourSeven.TryParse("24/7 closed");
            Assert.Equal("closed", r247.StateAt(DateTime.Now, "x"));

            var weekday = DaysOfWeekRule.TryParse("mo-fr 09:00-12:00 13:00-17:00");

            // Mo
            var state = weekday.StateAt(new DateTime(2019, 04, 22, 16, 00, 00), "closed");
            Assert.Equal("open", state);
            
            state = weekday.StateAt(new DateTime(2019, 04, 22, 17, 01, 00), "closed");
            Assert.Equal("closed", state);
            
            state = weekday.StateAt(new DateTime(2019, 04, 22, 12, 30, 00), "closed");
            Assert.Equal("closed", state);
            
                        
            state = weekday.StateAt(new DateTime(2019, 04, 22, 11, 30, 00), "closed");
            Assert.Equal("open", state);
            // Tue
            state = weekday.StateAt(new DateTime(2019, 04, 23, 16, 00, 00), "closed");
            Assert.Equal("open", state);
            
            // We
            state = weekday.StateAt(new DateTime(2019, 04, 24, 16, 00, 00), "closed");
            Assert.Equal("open", state);
            
            // Th
            state = weekday.StateAt(new DateTime(2019, 04, 25, 16, 00, 00), "closed");
            Assert.Equal("open", state);
            
            // Fr
            state = weekday.StateAt(new DateTime(2019, 04, 26, 16, 00, 00), "closed");
            Assert.Equal("open", state);
            // Sa
            state = weekday.StateAt(new DateTime(2019, 04, 27, 16, 00, 00), "closed");
            Assert.Equal("closed", state);
            // Su
            state = weekday.StateAt(new DateTime(2019, 04, 28, 16, 00, 00), "closed");
            Assert.Equal("closed", state);
            
        }
    }
}