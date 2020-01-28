using System;
using Itinero.Transit.Algorithms.Filter;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Core;
using Itinero.Transit.Journey.Metric;
using Itinero.Transit.OtherMode;
using Xunit;

namespace Itinero.Transit.Tests.Core.Algorithms.CSA
{
    public class JourneyFilterTest
    {
        [Fact]
        public void CalculateJourneys_SmallTdb_MaxNumberOfTransferFilter_ExpextsOneJourneyWithoutTransfers()
        {
            var tdb = new TransitDb(0);

            var writer = tdb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (0.0, 0.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.1, 1.1)));
            var stop2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (0.5, 0.5)));
            var date = new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc);


            writer.AddOrUpdateConnection(new Connection(stop0, stop1,
                "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc),
                10 * 60, new TripId(0, 0), 0));

            writer.AddOrUpdateConnection(new Connection(stop1, stop2,
                "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 16, 33, 00, DateTimeKind.Utc),
                10 * 60, new TripId(0, 1), 0));


            writer.AddOrUpdateConnection(new Connection(stop0, stop2,
                "https://example.com/connections/2",
                new DateTime(2018, 12, 04, 16, 25, 00, DateTimeKind.Utc),
                30 * 60, new TripId(0, 2), 0));

            tdb.CloseWriter();


            var routerWithTransfer = tdb.SelectProfile(new DefaultProfile())
                .SelectStops(stop0, stop2)
                .SelectTimeFrame(date, date.AddHours(1));


            var profile = new Profile<TransferMetric>(
                new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare,
                null,
                new MaxNumberOfTransferFilter(0)
            );


            var router = tdb.SelectProfile(profile)
                .SelectStops(stop0, stop2)
                .SelectTimeFrame(date, date.AddHours(1));


            var pcsTr = routerWithTransfer.CalculateAllJourneys();
            Assert.Equal(2, pcsTr.Count);
            Assert.Equal((uint) 2, pcsTr[0].Metric.NumberOfVehiclesTaken);
            Assert.Equal((uint) 1, pcsTr[1].Metric.NumberOfVehiclesTaken);

            var pcs = router.CalculateAllJourneys();
            Assert.NotNull(pcs);
            Assert.Single(pcs);
            Assert.Equal((uint) 1, pcs[0].Metric.NumberOfVehiclesTaken);
        }

        [Fact]
        public void CalculateJourneys_SmallTdb_CancelledConnectionFilter_ExpectsJourneyWithoutCancelledConnections()
        {
            var tdb = new TransitDb(0);

            var writer = tdb.GetWriter();

            var stop0 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/0", (0.0, 0.0)));
            var stop1 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/1", (0.1, 1.1)));
            var stop2 = writer.AddOrUpdateStop(new Stop("https://example.com/stops/2", (0.5, 0.5)));
            var date = new DateTime(2018, 12, 04, 16, 00, 00, DateTimeKind.Utc);


            writer.AddOrUpdateConnection(new Connection(stop0, stop1,
                "https://example.com/connections/0",
                new DateTime(2018, 12, 04, 16, 20, 00, DateTimeKind.Utc),
                10 * 60, new TripId(0, 0), 0));

            writer.AddOrUpdateConnection(new Connection(stop1, stop2,
                "https://example.com/connections/1",
                new DateTime(2018, 12, 04, 16, 33, 00, DateTimeKind.Utc),
                10 * 60, new TripId(0, 1), 0));


            // Faster, better, stronger... but cancelled
            writer.AddOrUpdateConnection(new Connection(stop0, stop2,
                "https://example.com/connections/2",
                new DateTime(2018, 12, 04, 16, 10, 00, DateTimeKind.Utc),
                10 * 60, new TripId(0, 2), 4));

            tdb.CloseWriter();


            //Filters the cancelled connections because of DefaultProfile
            var routerWithTransfer = tdb.SelectProfile(new DefaultProfile())
                .SelectStops(stop0, stop2)
                .SelectTimeFrame(date, date.AddHours(1));


            var easTr = routerWithTransfer.CalculateEarliestArrivalJourney();
            Assert.Equal((uint) 2, easTr.Metric.NumberOfVehiclesTaken);

            routerWithTransfer.ResetFilter();

            var pcsTr = routerWithTransfer.CalculateAllJourneys();
            Assert.Single(pcsTr);
            Assert.Equal((uint) 2, pcsTr[0].Metric.NumberOfVehiclesTaken);


            var profile = new Profile<TransferMetric>(
                new InternalTransferGenerator(),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory,
                TransferMetric.ParetoCompare
            );


            var router = tdb.SelectProfile(profile)
                .SelectStops(stop0, stop2)
                .SelectTimeFrame(date, date.AddHours(1));

            var pcs = router.CalculateAllJourneys();
            Assert.Equal(2,pcs.Count);
            Assert.Equal((uint) 1, pcs[0].Metric.NumberOfVehiclesTaken);
            Assert.Equal((uint) 2, pcs[1].Metric.NumberOfVehiclesTaken);
        }
    }
}