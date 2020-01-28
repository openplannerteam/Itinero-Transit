using System;
using System.Collections.Generic;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Core;
using Itinero.Transit.IO.OSM.Data;
using Itinero.Transit.Journey.Metric;
using Itinero.Transit.OtherMode;
using Xunit;

namespace Itinero.Transit.Tests.IO.OSM
{
    public class EasWithOsmFloatingPointsTest
    {
        [Fact]
        public void EarliestArrival_WithBeginWalk_ExpectsJourney()
        {
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1",
                (0.000001, 0.00001))); // very walkable distance


            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));

            transitDb.CloseWriter();

            var input = transitDb
                .SelectProfile(new DefaultProfile())
                .SelectStops((50.0, 50.0), (0.0, 0.0))
                .SelectTimeFrame(
                    new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 10, 00, 00, DateTimeKind.Utc));

            var eas = input.CalculateEarliestArrivalJourney();
            Assert.NotNull(eas);
        }


        [Fact]
        public void Earliest_WithBeginWalk_ExpectsJourney()
        {
            var input = OneConnectionDbWithBeginWalk();

            var journey = input.CalculateEarliestArrivalJourney();
            Assert.NotNull(journey);
            input.ResetFilter();
        }

        [Fact]
        public void Latest_WithBeginWalk_ExpectsJourney()
        {
            var input = OneConnectionDbWithBeginWalk();

            var las = input.CalculateLatestDepartureJourney();
            Assert.NotNull(las);
            input.ResetFilter();
        }


        [Fact]
        public void PCS_WithBeginWalk_ExpectsJourney()
        {
            var input = OneConnectionDbWithBeginWalk();

            var pcs = input.CalculateAllJourneys();
            Assert.NotNull(pcs);
            Assert.Single(pcs);
        }


        private static WithTime<TransferMetric> OneConnectionDbWithBeginWalk()
        {
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1",
                (0.000001, 0.00001))); // very walkable distance


            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));

            transitDb.CloseWriter();


            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var coor = (50.00005, 49.99953);
            var osmStopReader = new OsmStopsDb(1, new List<(double lon, double lat)> {coor});
            var departureLocation = osmStopReader.FindClosest(coor, 50).GlobalId;

            // Walk from start
            var input = latest
                .SelectProfile(profile)
                .AddStopsReader(osmStopReader)
                .SelectStops(departureLocation, "https://example.com/stops/1")
                .SelectTimeFrame(
                    latest.EarliestDate().AddMinutes(-60),
                    latest.LatestDate().AddMinutes(60));
            return input;
        }


        [Fact]
        public void EarliestLatestAll_WithEndWalk_ExpectsJourney()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1",
                (0.000001, 0.00001))); // very walkable distance

            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));


            transitDb.CloseWriter();

            var latest = transitDb.Latest;
            var arrivalLocation = "https://www.openstreetmap.org/#map=19/0.0/0.0";


            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var osmStopReader = new OsmStopsDb(1, new (double lon, double lat)[] {(0, 0)});

            // Walk to end

            var input = latest
                .SelectProfile(profile)
                .AddStopsReader(osmStopReader)
                .SelectStops("https://example.com/stops/0", arrivalLocation)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc));


            var journey = input.CalculateEarliestArrivalJourney();
            Assert.NotNull(journey);
            input.ResetFilter();
            var las = input.CalculateLatestDepartureJourney();
            Assert.NotNull(las);
            input.ResetFilter();
            var pcs = input.CalculateAllJourneys();
            Assert.NotNull(pcs);
            Assert.Single(pcs);
        }

        [Fact]
        public void Earliest_WithBeginAndEndWalk_ExpectsJourney()
        {
            var input = OneConnectionDbWithBeginAndEndWalk();

            var journey = input.CalculateEarliestArrivalJourney();
            Assert.NotNull(journey);
            input.ResetFilter();
        }

        [Fact]
        public void Latest_WithBeginAndEndWalk_ExpectsJourney()
        {
            var input = OneConnectionDbWithBeginAndEndWalk();

            var las = input.CalculateLatestDepartureJourney();
            Assert.NotNull(las);
            input.ResetFilter();
        }

        [Fact]
        public void PCS_WithBeginAndEndWalk_ExpectsJourney()
        {
            var input = OneConnectionDbWithBeginAndEndWalk();

            var pcs = input.CalculateAllJourneys();
            Assert.NotNull(pcs);
            Assert.Single(pcs);
        }

        private static WithTime<TransferMetric> OneConnectionDbWithBeginAndEndWalk()
        {
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1",
                (0.000001, 0.00001))); // very walkable distance

            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));


            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            var coor0 = (50.00005, 49.99953);
            var coor1 = (0, 0);
            var osmStopReader = new OsmStopsDb(1, new List<(double lon, double lat)>
            {
                coor0, coor1
            });

            var departureLocation = osmStopReader.FindClosest(coor0, 50);
            var arrivalLocation = osmStopReader.FindClosest(coor1, 50);

            var input = latest
                .SelectProfile(profile)
                .AddStopsReader(osmStopReader)
                .SelectStops(departureLocation, arrivalLocation)
                .SelectTimeFrame(
                    latest.EarliestDate().AddMinutes(-20), latest.LatestDate().AddMinutes(20)
                );
            return input;
        }
    }
}