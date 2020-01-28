using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Transit.Algorithms.CSA;
using Itinero.Transit.Algorithms.Filter;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Core;
using Itinero.Transit.Journey.Metric;
using Itinero.Transit.OtherMode;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace Itinero.Transit.Tests.Core.Algorithms.CSA
{
    public class ProfiledConnectionScanTest
    {
        [Fact]
        public void AllJourneys_SingleConnectionTdb_JourneyWithBeginWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1",
                (0.000001, 0.00001))); // very walkable distance


            var w0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (50.00001, 50.00001)));

            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));


            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk from start
            var journeys = latest.SelectProfile(profile)
                .SelectStops(w0, stop1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Single(journeys);
        }


        [Fact]
        public void AllJourneys_SingleConnectionTdb_JourneyWithEndWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.000001,0.00001))); // very walkable distance

            var w1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/3", (0.000002,0.00002))); // very walkable distance

            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));


            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk to end
            var journeys = latest.SelectProfile(profile)
                .SelectStops(stop0, w1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Single(journeys);
        }

        [Fact]
        public void AllJourneys_SingleConnectionTdb_JourneyWithBeginAndEndWalk()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.000001,0.00001))); // very walkable distance
            var w0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (50.00001, 50.00001)));
            var w1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/3", (0.000002,0.00002))); // very walkable distance

            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));


            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk to end
            var journeys = latest.SelectProfile(profile)
                .SelectStops(w0, w1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Single(journeys);
        }


        [Fact]
        public void AllJourneys_SmallTdb_2Journeys()
        {
            var tdb = Db.GetDefaultTestDb(out var stop0, out _, out _, out var stop3, out var _, out var _);

            var db = tdb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(60),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory, TransferMetric.ParetoCompare);


            var journeys = db.SelectProfile(profile)
                .SelectStops(stop0, stop3)
                .SelectTimeFrame(
                    new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 18, 00, 00, DateTimeKind.Utc)
                ).CalculateAllJourneys();

            //Pr("---------------- DONE ----------------");
            foreach (var j in journeys)
            {
                //Pr(j.ToString());
                Assert.True(Equals(stop0, j.Root.Location));
                Assert.True(Equals(stop3, j.Location));
            }

            Assert.Equal(2, journeys.Count());
        }

        /// <summary>
        /// This test gives two possible routes to PCS:
        /// one which is clearly better then the other.
        /// </summary>
        [Fact]
        public static void AllJourneys_2SimilarConnectionsButOneIsFaster_ExpectsOneOptimalJourney()
        {
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var loc0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (0, 0.0)));
            var loc1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.1, 0.1)));

            var t0 = new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc);
            
            writer.AddOrUpdateConnection(new Connection(
                "https://example.com/connections/0",
                loc0, loc1,
                t0,
                30 * 60, 
                new TripId(0, 0)));


            writer.AddOrUpdateConnection(new Connection(
                "https://example.com/connections/1",
                loc0, loc1,
                t0,
                40 * 60, new TripId(0, 1)));

            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(
                new InternalTransferGenerator(60),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);

            var journeys = latest.SelectProfile(profile)
                .SelectStops(loc0, loc1)
                .SelectTimeFrame(latest.EarliestDate(), latest.LatestDate().AddMinutes(60))
                .CalculateAllJourneys();
            // string.Join("\n\n-----------------------\n\n", journeys.Select(j => j.ToString(15)))
            Assert.Single(journeys);
            foreach (var j in journeys)
            {
                Assert.Equal(30 * 60, (int) j.Metric.TravelTime);
            }
        }

        [Fact]
        public static void AllJourneys_4ConnectionTdbWithMetricGuesser_ExpectsOneOptimalJourney()
        {
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var loc0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (0, 0.0)));
            var loc1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.1, 0.1)));
            var loc2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (2.1, 0.1)));
            var loc3 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (3.1, 0.1)));

            writer.AddOrUpdateConnection(new Connection(loc0, loc1,
                "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                30 * 60, new TripId(0, 0), 0));


            writer.AddOrUpdateConnection(new Connection(loc0, loc1,
                "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                40 * 60, new TripId(0, 1), 0));

            writer.AddOrUpdateConnection(new Connection(loc2, loc3, "https//example.com/connections/2",
                new DateTime(2018, 12, 04, 20, 00, 00, DateTimeKind.Utc),
                40 * 60, new TripId(0, 2), 0));

            writer.AddOrUpdateConnection(new Connection(loc2, loc3, "https//example.com/connections/4",
                new DateTime(2018, 12, 04, 2, 00, 00, DateTimeKind.Utc),
                40 * 60, new TripId(0, 3), 0));

            transitDb.CloseWriter();


            var latest = transitDb.Latest;


            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(60),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory, TransferMetric.ParetoCompare);

            var calculator = latest.SelectProfile(profile)
                .SelectStops(loc0, loc1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 18, 00, 00, DateTimeKind.Utc));


            var settings = calculator.GetScanSettings();

            calculator.StopsDb.TryGetId(calculator.From[0].GlobalId, out var fromId);
            settings.MetricGuesser = new SimpleMetricGuesser<TransferMetric>(fromId);

            var pcs = new ProfiledConnectionScan<TransferMetric>(calculator.GetScanSettings());
            var journeys = pcs.CalculateJourneys();
            Assert.Single(journeys);
            foreach (var j in journeys)
            {
                Assert.Equal(30 * 60, (int) j.Metric.TravelTime);
            }
        }


        [Fact]
        public void AllJourneys_1ConnectionTdbWithNoGettingOfMode_ExpectsNoJourneys()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (0, 0.0)));
            var stop2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.1, 0.1)));

            writer.AddOrUpdateConnection(new Connection(stop1, stop2, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0),
                3)); // MODE 3 - cant get on or off

            transitDb.CloseWriter();
            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);
            var journey = latest.SelectProfile(profile)
                .SelectStops(stop1, stop2)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 19, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();

            Assert.Null(journey);
        }


        /// <summary>
        /// Regression test
        ///
        ///
        /// Kristof discovered a case where a huge crows flight took 7h and fell squarely out of the search window,
        /// even though other options were still available
        ///
        /// THis test tries to mimick it
        /// 
        /// </summary>
        [Fact]
        public void AllJourneys_1ConnectionTdbWalkRequired_ExpectsNoJourneyAsWalkFallsBeforeTimeWindow()
        {
            // Locations: loc0 -> loc2

            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var loc0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (3.1904983520507812,51.256758449834216)));
            var loc1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (3.216590881347656,51.197848510420464)));
            var loc2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (3.7236785888671875,51.05348088381823)));

            writer.AddOrUpdateConnection(new Connection(loc1, loc2,
                "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 01, 00, DateTimeKind.Utc),
                30 * 60, new TripId(0, 0), 0));

            transitDb.CloseWriter();

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(10000),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);
            var journeys = transitDb.SelectProfile(profile).SelectStops(loc0, loc2)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 17, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();

            Assert.Null(journeys);
        }

        /// <summary>
        /// Regression test
        ///
        ///
        /// Kristof discovered a case where a huge crows flight took 7h and fell squarely out of the search window,
        /// even though other options were still available
        ///
        /// THis test tries to mimick it
        /// 
        /// </summary>
        [Fact]
        public void AllJourneys_1ConnectionTdbWalkRequired_ExpectsNoJourneyAsWalkFallsAfterTimeWindow()
        {
            // Locations: loc0 -> loc2

            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var loc0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (3.1904983520507812,51.256758449834216)));
            var loc1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (3.216590881347656,51.197848510420464)));
            var loc2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (3.7236785888671875,51.05348088381823)));

            writer.AddOrUpdateConnection(new Connection(loc2, loc1,
                "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 01, 00, DateTimeKind.Utc),
                30 * 60, new TripId(0, 0), 0));

            transitDb.CloseWriter();

            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(10000),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);
            var journeys = transitDb.SelectProfile(profile).SelectStops(loc2, loc0)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 17, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();

            Assert.Null(journeys);
        }

        [Fact]
        public void AllJourneys_SingleConnectionTdb_JourneyWithNoWalkAndNoSearch()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.000001,0.00001))); // very walkable distance


            writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (50.00001, 50.00001)));

            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));


            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(
                new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(0),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk from start
            var journeys = latest.SelectProfile(profile)
                .SelectStops(stop0, stop1)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 11, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Single(journeys);
        }


        [Fact]
        public void AllJourneys_TwoConnectionDifferentTrip_JourneyWithTransfer()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.000001,0.00001))); // very walkable distance
            var stop2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (0.08,0.00001))); // very walkable distance


            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));

            writer.AddOrUpdateConnection(new Connection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 10, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 1), 0));

            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(
                new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(0),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk from start
            var journeys = latest.SelectProfile(profile)
                .SelectStops(stop0, stop2)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 12, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Single(journeys);
            Assert.Equal((uint) 2, journeys[0].Metric.NumberOfVehiclesTaken);
        }


        [Fact]
        public void AllJourneys_TwoConnectionSameTrip_JourneyWithExtendedTrip()
        {
            // build a one-connection db.
            var transitDb = new TransitDb(0);
            var writer = transitDb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (50, 50.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.000001,0.00001))); // very walkable distance
            var stop2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (0.08,
                0.00001))); // very walkable distance


            writer.AddOrUpdateConnection(new Connection(stop0, stop1, "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 9, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));

            writer.AddOrUpdateConnection(new Connection(stop1, stop2, "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 10, 30, 00, DateTimeKind.Utc), 10 * 60, new TripId(0, 0), 0));

            transitDb.CloseWriter();

            var latest = transitDb.Latest;

            var profile = new Profile<TransferMetric>(
                new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(0),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare);


            // Walk from start
            var journeys = latest.SelectProfile(profile)
                .SelectStops(stop0, stop2)
                .SelectTimeFrame(new DateTime(2018, 12, 04, 9, 00, 00, DateTimeKind.Utc),
                    new DateTime(2018, 12, 04, 12, 00, 00, DateTimeKind.Utc))
                .CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Single(journeys);
            Assert.Equal((uint) 1, journeys[0].Metric.NumberOfVehiclesTaken);
        }


        [Fact]
        public void AllJourneys_ThreeTransfersTwoFamilies_WithFiltering_Expects4Journeys()
        {
            var tdb = new TransitDb(0);

            var wr = tdb.GetWriter();

            var stops = new List<StopId>();
            for (var i = 0; i < 10; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }

            // Trip A: stop 0 -> 3
            // Trip B: stop 2 -> 4 (only B has 4) -> 6
            // Trip C: stop 5 -> 8
            var tripA = new TripId(0, 0);
            for (var i = 0; i < 3; i++)
            {
                // Departs at: 0, 1, 2
                // Arrives at: 1, 2, 3
                wr.AddOrUpdateConnection(new Connection("A" + i,
                    stops[i], stops[i + 1],
                    (ulong) (1000 + i * 60), 60, 0, tripA));
            }

            var tripB = new TripId(0, 1);
            for (var i = 2; i < 6; i++)
            {
                // Departs at: 2, 3, 4, 5
                // Arrives at: 3, 4, 5, 6
                wr.AddOrUpdateConnection(new Connection("B" + i, stops[i], stops[i + 1],
                    (ulong) (1000 + 240 + i * 60), 60, 0, tripB));
            }

            var tripC = new TripId(0, 2);
            for (var i = 5; i < 8; i++)
            {
                wr.AddOrUpdateConnection(new Connection("C" + i, stops[i], stops[i + 1],
                    (ulong) (1000 + 241 + i * 60), 60, 0, tripC));
            }

            tdb.CloseWriter();


            var pr = new DefaultProfile(0, 0);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(stops[0], stops[8])
                .SelectTimeFrame(0, 2000);


            calc.CheckAll();
            var settings = calc.GetScanSettings();

            settings.Stops.TryGetId(settings.DepartureStop[0].GlobalId, out var depId);
            settings.MetricGuesser = new SimpleMetricGuesser<TransferMetric>(depId);

            var pcs = new ProfiledConnectionScan<TransferMetric>(settings);
            var journeys = pcs.CalculateJourneys();

            Assert.NotNull(journeys);
            Assert.Equal(4, journeys.Count);
        }

        [Fact]
        public void AllJourneys_TripsGoAndReturn4Stops_ExpectsTwoOptions()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();
            var stops = new List<StopId>();
            for (var i = 0; i < 5; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }


            // Trip A
            wr.AddOrUpdateConnection(new Connection(stops[0], stops[1], "a0", 1000, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[2], "a1", 1060, 60, new TripId(0, 0)));

            // Trip B
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[1], "b1", 2060, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[3], "b2", 2120, 60, new TripId(0, 1)));

            tdb.CloseWriter();

            var pr = new DefaultProfile(0, 60);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(stops[0], stops[3])
                .SelectTimeFrame(0, 5000);

            var journeys = calc.CalculateAllJourneys();

            // Expected options - they are the same to the transfermetric
            // stop0 -> stop1, transfer, stop1 -> stop4
            // stop0 -> stop1 -> stop2, transfer, stop2 -> stop1 -> stop4

            Assert.NotNull(journeys);
            Assert.Equal(2, journeys.Count);
        }

        [Fact]
        public void AllJourneys_TripsGoAndReturn5stops_ExpectsThreeOptions()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();
            var stops = new List<StopId>();
            for (var i = 0; i < 5; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }


            // Trip A
            wr.AddOrUpdateConnection(new Connection(stops[0], stops[1], "a0", 1000, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[2], "a1", 1060, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[3], "a2", 1120, 60, new TripId(0, 0)));

            // Trip B
            wr.AddOrUpdateConnection(new Connection(stops[3], stops[2], "b0", 2000, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[1], "b1", 2060, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[4], "b2", 2120, 60, new TripId(0, 1)));

            tdb.CloseWriter();

            var pr = new DefaultProfile(0, 60);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(stops[0], stops[4])
                .SelectTimeFrame(0, 5000);

            var journeys = calc.CalculateAllJourneys();

            // Expected options - they are the same to the transfermetric
            // stop0 -> stop1, transfer, stop1 -> stop4
            // stop0 -> stop1 -> stop2, transfer, stop2 -> stop1 -> stop4

            Assert.NotNull(journeys);
            Assert.Equal(3, journeys.Count);
        }

        [Fact]
        public void AllJourneys_TripsGoAndReturn6stops_ExpectsThreeOptions()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();
            var stops = new List<StopId>();
            for (var i = 0; i < 6; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }


            // Trip A
            wr.AddOrUpdateConnection(new Connection(stops[0], stops[1], "a0", 1000, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[2], "a1", 1060, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[3], "a2", 1120, 60, new TripId(0, 0)));

            // Trip B
            wr.AddOrUpdateConnection(new Connection(stops[3], stops[2], "b0", 2000, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[1], "b1", 2060, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[4], "b2", 2120, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[4], stops[5], "b3", 2180, 60, new TripId(0, 1)));

            tdb.CloseWriter();

            var pr = new DefaultProfile(0, 60);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(stops[0], stops[5])
                .SelectTimeFrame(0, 5000);

            var journeys = calc.CalculateAllJourneys();

            // Expected options - they are the same to the transfermetric
            // stop0 -> stop1, transfer, stop1 -> stop4
            // stop0 -> stop1 -> stop2, transfer, stop2 -> stop1 -> stop4

            Assert.NotNull(journeys);
            Assert.Equal(3, journeys.Count);
        }


        [Fact]
        public void AllJourneys_TripsGoAndParallel5stops_ExpectsThreeOptions()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();
            var stops = new List<StopId>();
            for (var i = 0; i < 5; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }


            // Trip A
            wr.AddOrUpdateConnection(new Connection(stops[0], stops[1], "a0", 1000, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[2], "a1", 1060, 60, new TripId(0, 0)));
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[3], "a2", 1120, 60, new TripId(0, 0)));

            // Trip B
            wr.AddOrUpdateConnection(new Connection(stops[1], stops[2], "b0", 2000, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[2], stops[3], "b1", 2060, 60, new TripId(0, 1)));
            wr.AddOrUpdateConnection(new Connection(stops[3], stops[4], "b2", 2120, 60, new TripId(0, 1)));

            tdb.CloseWriter();

            var pr = new DefaultProfile(0, 60);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(stops[0], stops[4])
                .SelectTimeFrame(0, 5000);

            var journeys = calc.CalculateAllJourneys();

            Assert.NotNull(journeys);
            Assert.Equal(3, journeys.Count);
        }

        [Fact]
        public void AllJourneys_ConnectionTransferTwoOptions_ShouldReturn2()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();


            var stops = new List<StopId>();
            for (var i = 0; i < 12; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }

            var departure = wr.AddOrUpdateStop(new Stop("departure", (3.05, 9.05)));
            var arrival = wr.AddOrUpdateStop(new Stop("arrival", (2.55, 9.05)));
            var commonStop = wr.AddOrUpdateStop(new Stop("commonStop", (4.55, 9.55)));

            var commonTrip = new TripId(0, 0);
            wr.AddOrUpdateConnection(new Connection(departure, commonStop, "commonConn", 1000, 60, commonTrip));

            // Slow family, but less transfers. IT waits at the common stop for a longer time
            var slowA = wr.AddOrUpdateStop(new Stop("slowA", (13.55, 0.95)));
            var slowB = wr.AddOrUpdateStop(new Stop("slowB", (13.55, 0.05)));
            var slowTrip0 = new TripId(0, 4);

            wr.AddOrUpdateConnection(new Connection(commonStop, stops[10], "slow0", 2500, 60, slowTrip0));
            wr.AddOrUpdateConnection(new Connection(stops[10], slowA, "slow1", 2600, 60, slowTrip0));
            wr.AddOrUpdateConnection(new Connection(slowA, slowB, "slow2", 2700, 60, slowTrip0));


            var slowTrip1 = new TripId(0, 5);

            wr.AddOrUpdateConnection(new Connection(slowA, slowB, "slowA0", 2800, 60, slowTrip1));
            wr.AddOrUpdateConnection(new Connection(slowB, stops[11], "slowA1", 2900, 60, slowTrip1));

            wr.AddOrUpdateConnection(new Connection(stops[11], arrival, "slowA2", 3000, 60, slowTrip1));

            tdb.CloseWriter();

            var pr = new DefaultProfile(0, 60);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(commonStop, arrival)
                .SelectTimeFrame(0, 10000);

            var journeys = calc.CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Equal(2, journeys.Count);

            calc = tdb.SelectProfile(pr)
                .SelectStops(departure, arrival)
                .SelectTimeFrame(0, 10000);
            journeys = calc.CalculateAllJourneys();
            Assert.NotNull(journeys);
            Assert.Equal(2, journeys.Count);

            calc = tdb.SelectProfile(pr)
                .SelectStops(departure, arrival)
                .SelectTimeFrame(0, 10000);

            calc.CheckAll();
            var settings = calc.GetScanSettings();
            settings.Stops.TryGetId(settings.DepartureStop[0].GlobalId, out var depId);

            settings.MetricGuesser = new SimpleMetricGuesser<TransferMetric>(depId);

            var pcs = new ProfiledConnectionScan<TransferMetric>(settings);
            journeys = pcs.CalculateJourneys();
            Assert.NotNull(journeys);
            Assert.Equal(2, journeys.Count);
        }

        [Fact]
        public void AllJourneys_SpecialTransferSequence_ShouldReturn4()
        {
            var tdb = new TransitDb(0);
            var wr = tdb.GetWriter();


            var stops = new List<StopId>();
            for (var i = 0; i < 12; i++)
            {
                stops.Add(wr.AddOrUpdateStop(new Stop("stop" + i, (i, i))));
            }

            var departure = wr.AddOrUpdateStop(new Stop("departure", (3.05, 0.05)));
            var arrival = wr.AddOrUpdateStop(new Stop("arrival", (2.55, 0.05)));

            var commonStop = wr.AddOrUpdateStop(new Stop("commonStop", (4.55, 0.55)));


            // There are a few possible journeys from 
            // There is one 'fast' family with an extra vehicle (3), and one slower family with 2 transfers.
            // Both depart at the same time

            // They both arrive at the same intermediate stop, where they both transfer and continue on a common edge

            // Common part
            var commonTrip = new TripId(0, 0);
            wr.AddOrUpdateConnection(new Connection(departure, commonStop, "commonConn", 1000, 60, commonTrip));

            // Fast family, but with a few extra transfers

            var fastA = wr.AddOrUpdateStop(new Stop("fastA", (1.55, 0.05)));
            var fastB = wr.AddOrUpdateStop(new Stop("fastB", (1.55, 0.35)));

            // The extra stop
            var fastC = wr.AddOrUpdateStop(new Stop("fastC", (1.55, 0.95)));


            var fastTrip0 = new TripId(0, 1);
            wr.AddOrUpdateConnection(new Connection(commonStop, stops[0], "fast0", 1500, 60, fastTrip0));
            wr.AddOrUpdateConnection(new Connection(stops[0], fastA, "fast1", 1600, 60, fastTrip0));
            wr.AddOrUpdateConnection(new Connection(fastA, fastB, "fast2", 1700, 60, fastTrip0));


            var fastTrip1 = new TripId(0, 2);
            wr.AddOrUpdateConnection(new Connection(fastA, fastB, "fastA0", 1800, 60, fastTrip1));
            wr.AddOrUpdateConnection(new Connection(fastB, stops[1], "fastA1", 1900, 60, fastTrip1));
            wr.AddOrUpdateConnection(new Connection(stops[1], fastC, "fastA2", 2000, 60, fastTrip1));

            var fastTrip2 = new TripId(0, 3);
            wr.AddOrUpdateConnection(new Connection(fastC, stops[2], "fastB0", 2100, 60, fastTrip2));
            wr.AddOrUpdateConnection(new Connection(stops[2], arrival, "fastB1", 2200, 60, fastTrip2));

            // Slow family, but less transfers. IT waits at the common stop for a longer time
            var slowA = wr.AddOrUpdateStop(new Stop("slowA", (13.55, 0.95)));
            var slowB = wr.AddOrUpdateStop(new Stop("slowB", (13.55, 0.05)));
            var slowTrip0 = new TripId(0, 4);

            wr.AddOrUpdateConnection(new Connection(commonStop, stops[10], "slow0", 2500, 60, slowTrip0));
            wr.AddOrUpdateConnection(new Connection(stops[10], slowA, "slow1", 2600, 60, slowTrip0));
            wr.AddOrUpdateConnection(new Connection(slowA, slowB, "slow2", 2700, 60, slowTrip0));


            var slowTrip1 = new TripId(0, 5);

            wr.AddOrUpdateConnection(new Connection(slowA, slowB, "slowA0", 2800, 60, slowTrip1));
            wr.AddOrUpdateConnection(new Connection(slowB, stops[11], "slowA1", 2900, 60, slowTrip1));

            wr.AddOrUpdateConnection(new Connection(stops[11], arrival, "slowA2", 3000, 60, slowTrip1));

            tdb.CloseWriter();

            var pr = new DefaultProfile(0, 60);

            var calc = tdb.SelectProfile(pr)
                .SelectStops(commonStop, arrival)
                .SelectTimeFrame(0, 10000);


            calc.CheckAll();
            var settings = calc.GetScanSettings();

            settings.MetricGuesser = null;

            var pcs = new ProfiledConnectionScan<TransferMetric>(settings);
            var journeys = pcs.CalculateJourneys();


            Assert.NotNull(journeys);
            Assert.Equal(4, journeys.Count);


            calc = tdb.SelectProfile(pr)
                .SelectStops(commonStop, arrival)
                .SelectTimeFrame(0, 10000);


            calc.CheckAll();
            settings = calc.GetScanSettings();
            settings.Stops.TryGetId(settings.DepartureStop[0].GlobalId, out var depId);

            settings.MetricGuesser =
                new SimpleMetricGuesser<TransferMetric>(depId);

            pcs = new ProfiledConnectionScan<TransferMetric>(settings);
            journeys = pcs.CalculateJourneys();


            Assert.NotNull(journeys);
            Assert.Equal(4, journeys.Count);
        }
    }
}