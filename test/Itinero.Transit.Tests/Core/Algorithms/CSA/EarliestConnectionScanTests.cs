using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Core;
using Itinero.Transit.Journey;
using Itinero.Transit.Journey.Metric;
using Itinero.Transit.OtherMode;
using Itinero.Transit.Utils;
using Xunit;

namespace Itinero.Transit.Tests.Core.Algorithms.CSA
{
    public class EarliestConnectionScanTests
    {
        [Fact]
        public void WithBeginWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop("https://example.com/stops/0", 50, 50.0);
            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.000001,
                0.00001); // very walkable distance


            var w0 = writer.AddOrUpdateStop("https://example.com/stops/2", 50.00001, 50.00001);

            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);


            writer.Close();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk from start
            var journey = latest.SelectProfile(profile)
                .SelectStops(w0, stop1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();
            Assert.NotNull(journey);
        }


        [Fact]
        public void WithEndWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop("https://example.com/stops/0", 50, 50.0);
            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.000001,
                0.00001); // very walkable distance

            var w1 = writer.AddOrUpdateStop("https://example.com/stops/3", 0.000002, 0.00002); // very walkable distance

            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);


            writer.Close();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk to end
            var journey = latest.SelectProfile(profile)
                .SelectStops(stop0, w1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();
            Assert.NotNull(journey);
        }

        [Fact]
        public void WithStartEndWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop("https://example.com/stops/0", 50, 50.0);
            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.000001,
                0.00001); // very walkable distance
            var w0 = writer.AddOrUpdateStop("https://example.com/stops/2", 50.00001, 50.00001);

            var w1 = writer.AddOrUpdateStop("https://example.com/stops/3", 0.000002, 0.00002); // very walkable distance

            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);


            writer.Close();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk to end
            var journey = latest.SelectProfile(profile)
                .SelectStops(w0, w1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();
            Assert.NotNull(journey);
        }

        [Fact]
        public void SimpleEasTest()
        {
            var tdb = Db.GetDefaultTestDb(out var stop0, out var stop1, out var stop2, out var _, out var _, out var _);
            var db = tdb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                null,
                TransferMetric.Factory,
                TransferMetric.ParetoCompare
            );


            var j = db
                .SelectProfile(profile)
                .SelectStops(stop0, stop1)
                .SelectTimeFrame(db.GetConn(0).DepartureTime.FromUnixTime(),
                    (db.GetConn(0).DepartureTime + 60 * 60 * 6).FromUnixTime())
                .EarliestArrivalJourney();

            Assert.NotNull(j);
            Assert.Equal(new ConnectionId(0, 0), j.Connection);


            j = db.SelectProfile(profile)
                    .SelectStops(stop0, stop2)
                    .SelectTimeFrame(
                        db.GetConn(0).DepartureTime.FromUnixTime(),
                        (db.GetConn(0).DepartureTime + 60 * 60 * 2).FromUnixTime())
                    .EarliestArrivalJourney()
                ;

            Assert.NotNull(j);
            Assert.Equal(new ConnectionId(0, 1), j.Connection);
        }

        [Fact]
        public void SimpleNotGettingOffTest()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop("https://example.com/stops/0", 50, 50.0);
            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/1", 0, 0.0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/2", 0.001, 0.001); // very walkable distance
            var stop3 = writer.AddOrUpdateStop("https://example.com/stops/3", 60.1, 60.1);

            // Note that all connections have mode '3', indicating neither getting on or of the connection
            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 10, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 3);
            writer.AddOrUpdateConnection(stop2, stop3, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 10, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 1), 3);

            // Prevent depletion of the DB
            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/2",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 2), 3);

            writer.Close();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var journey = latest.SelectProfile(profile)
                .SelectStops(stop0, stop3)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 10, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();

            // It is not possible to get on or off any connection
            // So we should not find anything
            Assert.Null(journey);
        }


        [Fact]
        public void WithIntermediateWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop("https://example.com/stops/0", 50, 50.0);
            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/1", 0, 0.0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/2", 0.000001,
                0.00001); // very walkable distance
            var stop3 = writer.AddOrUpdateStop("https://example.com/stops/3", 60.1, 60.1);

            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 10, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);
            writer.AddOrUpdateConnection(stop2, stop3, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 10, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 1), 0);

            // Prevent depletion of the DB
            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/2",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 2), 0);

            writer.Close();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);
            var journey = latest.SelectProfile(profile)
                .SelectStops(stop0, stop3)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();
            Assert.NotNull(journey);
            Assert.Equal(Journey<TransferMetric>.OTHERMODE, journey.PreviousLink.Connection);
            Assert.True(journey.PreviousLink.SpecialConnection);
        }

        [Fact]
        public void ShouldFindOneConnectionJourney()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/0", 0, 0.0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.1, 0.1);

            writer.AddOrUpdateConnection(
                stop1, stop2, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            // Prevent depletion of the DB
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 1), 0);

            writer.Close();

            var latest = transitDb.Latest;
            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                null,
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            var journey = latest.SelectProfile(profile)
                .SelectStops(stop1, stop2)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 19, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();

            Assert.NotNull(journey);
            Assert.Equal(2, journey.AllParts().Count());
        }

        [Fact]
        public void ShouldFindOneConnectionJourneyWithArrivalTravelTime()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/0", 0, 0.0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.1, 0.1);

            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            // Prevent depletion of the DB
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            writer.Close();
            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                null,
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var sources = new List<(StopId, Journey<TransferMetric> journey)>
                {(stop1, null)};
            var targets = new List<(StopId, Journey<TransferMetric> journey)>
            {
                (stop2,
                    new Journey<TransferMetric>(stop2, 0, profile.MetricFactory, new TripId(0, 42))
                        .ChainSpecial(Journey<TransferMetric>.OTHERMODE, 100, stop2, new TripId(0, 0))
                )
            };

            var latest = transitDb.Latest;

            var journey = latest
                    .SelectProfile(profile)
                    .SelectStops(sources, targets)
                    .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                        new DateTime(2018, 12, 04, 19, 00, 00, DateTimeKind.Utc))
                    .EarliestArrivalJourney()
                ;

            Assert.NotNull(journey);
            Assert.Equal(3, journey.AllParts().Count);
            Assert.Equal(Journey<TransferMetric>.OTHERMODE, journey.Connection);
            Assert.True(journey.SpecialConnection);
            Assert.False(journey.PreviousLink.SpecialConnection);
            Assert.Equal(new ConnectionId(0, 0), journey.PreviousLink.Connection);
            Assert.Equal(100, journey.Metric.WalkingTime);
            Assert.Equal((uint) (10 * 60 + 100), journey.Metric.TravelTime);
        }

        [Fact]
        public void ShouldFindOneConnectionJourneyWithDepartureTravelTime()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/0", 0, 0.0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.5, 0.5);

            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            // Prevent depletion of the DB
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            writer.Close();
            var latest = transitDb.Latest;


            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                null,
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            var startTime = new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc);
            var sources = new List<(StopId, Journey<TransferMetric> journey)>
            {
                (stop1,
                    new Journey<TransferMetric>(stop1, startTime.ToUnixTime(), profile.MetricFactory, new TripId(0, 42))
                        .ChainSpecial(Journey<TransferMetric>.OTHERMODE,
                            startTime.ToUnixTime() + 1000, stop1, tripId: new TripId(0, 42))
                )
            };

            var targets = new List<(StopId, Journey<TransferMetric> journey)>
                {(stop2, null)};


            var journey = latest.SelectProfile(profile)
                    .SelectStops(sources, targets)
                    .SelectTimeFrame(startTime, new DateTime(2018, 12, 04, 19, 00, 00, DateTimeKind.Utc))
                    .EarliestArrivalJourney()
                ;

            Assert.NotNull(journey);
            Assert.Equal(3, journey.AllParts().Count);
            Assert.Equal(new ConnectionId(0, 0), journey.Connection);
            Assert.True(journey.PreviousLink.PreviousLink.SpecialConnection);
            Assert.Equal(Journey<TransferMetric>.OTHERMODE,
                journey.PreviousLink.Connection);

            Assert.Equal((uint) 1, journey.Metric.NumberOfTransfers);
            Assert.Equal(1000, journey.Metric.WalkingTime);
            Assert.Equal((uint) 30 * 60, journey.Metric.TravelTime);
        }

        [Fact]
        public void TestNoOverscan()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/0", 0, 0.0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.1, 0.1);

            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 17, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/2",
                new DateTime(2018, 12, 04, 18, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/3",
                new DateTime(2018, 12, 04, 19, 20, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/4",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0), 0);

            writer.Close();

            var latest = transitDb.Latest;
            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                null,
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var journey = latest.SelectProfile(profile)
                .SelectStops(stop1, stop2)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 19, 00, 00, DateTimeKind.Utc))
                .EarliestArrivalJourney();
            Assert.NotNull(journey);
            Assert.Equal(2, journey.AllParts().Count);
        }

        [Fact]
        public void TestModes()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop("https://example.com/stops/0", 50, 50.0);
            var stop1 = writer.AddOrUpdateStop("https://example.com/stops/1", 0.0, 0);
            var stop2 = writer.AddOrUpdateStop("https://example.com/stops/2", 5, 10);


            writer.AddOrUpdateConnection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 10, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0),
                Connection.ModeGetOnOnly);
            writer.AddOrUpdateConnection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, 0, 0, new TripId(0, 0),
                Connection.ModeGetOffOnly);

            writer.Close();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var input = latest
                    .SelectProfile(profile)
                    .SelectStops(stop0, stop1)
                    .SelectTimeFrame(
                        new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                        new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                ;
            Assert.Null(input.EarliestArrivalJourney());

            input = latest
                    .SelectProfile(profile)
                    .SelectStops(stop1, stop0)
                    .SelectTimeFrame(
                        new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                        new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                ;
            Assert.Null(input.EarliestArrivalJourney());
            input = latest
                    .SelectProfile(profile)
                    .SelectStops(stop0, stop2)
                    .SelectTimeFrame(
                        new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                        new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                ;
            Assert.NotNull(input.EarliestArrivalJourney());
        }

        /// <summary>
        /// Another regression test from Kristof
        ///
        /// Earliest arrival scan sometimes selects the following:
        /// Departure at location A
        /// go to location B with train 0
        /// get on train 1
        /// go to A again, but stay seated
        /// continue to Destination
        /// 
        /// </summary>
        [Fact]
        public void ViaStartLocationAgain()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();

            var departure = wr.AddOrUpdateStop("departure", 0.0, 0.0);
            var arrival = wr.AddOrUpdateStop("arrival",1.0,1.0);

            var detour = wr.AddOrUpdateStop("detour", 2.0, 2.0);

            var trdetour = wr.AddOrUpdateTrip("tripDetour");
            var trdirect = wr.AddOrUpdateTrip("tripDirect");

            
            
            var c = new Connection
            {
                DepartureStop = departure,
                ArrivalStop = detour,
                DepartureTime = 1000,
                ArrivalTime = 1100,
                GlobalId = "a",
                TravelTime = 100,
                TripId = trdetour
            };
            
            wr.AddOrUpdateConnection(c);
            c = new Connection
            {
                DepartureStop = detour,
                ArrivalStop = departure,
                DepartureTime = 1500,
                ArrivalTime = 1600,
                GlobalId = "b",
                TravelTime = 100,
                TripId = trdirect
            };
            wr.AddOrUpdateConnection(c);
            
            c = new Connection
            {
                DepartureStop = departure,
                ArrivalStop = arrival,
                DepartureTime = 1700,
                ArrivalTime = 1800,
                GlobalId = "c",
                TravelTime = 100,
                TripId = trdirect
            };
            wr.AddOrUpdateConnection(c);
            wr.Close();

            var j = tdb.SelectProfile(new DefaultProfile())
                .SelectStops(departure, arrival)
                .SelectTimeFrame(1000, 2000)
                .EarliestArrivalJourney();

            // Only one connection should be used
            Assert.True(Equals(j.PreviousLink.Root, j.PreviousLink));
        }
    }
}