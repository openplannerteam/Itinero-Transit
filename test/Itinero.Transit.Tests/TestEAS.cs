using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Transit.CSA;
using Itinero.Transit.CSA.ConnectionProviders;
using Itinero.Transit.CSA.LocationProviders;
using Itinero.Transit_Tests;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global

namespace Itinero.Transit.Tests
{
    public class TestEas
    {
        private readonly ITestOutputHelper _output;

        public static Uri BrusselZuid = new Uri("http://irail.be/stations/NMBS/008814001");
        public static Uri Gent = new Uri("http://irail.be/stations/NMBS/008892007");
        public static Uri Brugge = new Uri("http://irail.be/stations/NMBS/008891009");
        public static Uri Poperinge = new Uri("http://irail.be/stations/NMBS/008896735");
        public static Uri Vielsalm = new Uri("http://irail.be/stations/NMBS/008845146");


        public TestEas(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestEarliestArrival()
        {
            var sncb = Sncb.Profile(ResourcesTest.TestPath, "belgium.routerdb");
            var startTime = ResourcesTest.TestMoment(10, 10);
            var endTime = ResourcesTest.TestMoment(23, 00);

            var csa = new EarliestConnectionScan<TransferStats>(Brugge, Gent, startTime, endTime, sncb);

            var journey = csa.CalculateJourney();
            Log(journey.ToString());
            Assert.Equal($"{ResourcesTest.TestMoment(10,24):O}", $"{journey.Connection.DepartureTime():O}");
            Assert.Equal("00:26:00", journey.Stats.TravelTime.ToString());
            Assert.Equal(0, journey.Stats.NumberOfTransfers);
        }


        [Fact]
        public void TestEarliestArrival2()
        {
            var sncb = Sncb.Profile(ResourcesTest.TestPath, "belgium.routerdb");

            var startTime = ResourcesTest.TestMoment(10, 08);
            var endTime = ResourcesTest.TestMoment(23, 00);
            var csa = new EarliestConnectionScan<TransferStats>(
                Poperinge, Vielsalm, startTime, endTime, sncb);
            var journey = csa.CalculateJourney();
            Log(journey.ToString(sncb));

            Assert.Equal($"{ResourcesTest.TestMoment(16,01):O}", $"{journey.Connection.DepartureTime():O}");
            Assert.Equal("06:05:00", journey.Stats.TravelTime.ToString());
            Assert.Equal(4, journey.Stats.NumberOfTransfers);
        }

        [Fact]
        public void TestDeLijn()
        {
            var deLijn = DeLijn.Profile(ResourcesTest.TestPath, "belgium.routerdb");
            Log("Got profile");
            var closeToHome = deLijn.LocationProvider.GetLocationsCloseTo(51.21576f, 3.22f, 250);

            var closeToTarget = deLijn.LocationProvider.GetLocationsCloseTo(51.19738f, 3.21736f, 500);
            Log("Found stops");

            Assert.Equal(6, closeToHome.Count());
            Assert.Equal(16, closeToTarget.Count());

            Assert.True(closeToHome.Contains(new Uri("https://data.delijn.be/stops/502101")));

            var startTime = ResourcesTest.TestMoment(10, 00);
            var endTime = ResourcesTest.TestMoment(11, 00);

            var startJourneys = new List<Journey<TransferStats>>();
            foreach (var uri in closeToHome)
            {
                startJourneys.Add(new Journey<TransferStats>(uri, startTime, TransferStats.Factory));
                Log($"> {uri} {deLijn.LocationProvider.GetNameOf(uri)}");
            }

            foreach (var uri in closeToTarget)
            {
                Log($"< {uri} {deLijn.LocationProvider.GetNameOf(uri)}");
            }
            
            var eas = new EarliestConnectionScan<TransferStats>(
                startJourneys, new List<Uri>(closeToTarget),
                deLijn, endTime);
            Log("Starting AES");
            var j = eas.CalculateJourney();
            Log(j.ToString(deLijn));
            Assert.Equal(0, j.Stats.NumberOfTransfers);
            Assert.Equal(7, (j.Stats.EndTime - j.Stats.StartTime).TotalMinutes);
            Log("Done");
        }

        private void Log(string s)
        {
            _output.WriteLine(s);
        }
    }
}