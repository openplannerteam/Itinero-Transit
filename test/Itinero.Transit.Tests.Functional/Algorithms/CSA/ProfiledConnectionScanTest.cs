using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.IO.LC;
using Itinero.Transit.Algorithms.CSA;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Walks;
using Itinero.Transit.Journeys;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace Itinero.Transit.Tests.Functional.Algorithms.CSA
{
    public class ProfiledConnectionScanTest : FunctionalTest<IEnumerable<Journey<TransferStats>>, (ConnectionsDb
        connections, StopsDb
        stops,
        string departureStopId, string arrivalStopId, DateTime departureTime)>
    {
        public static ProfiledConnectionScanTest Default => new ProfiledConnectionScanTest();

        protected override IEnumerable<Journey<TransferStats>> Execute((ConnectionsDb connections, StopsDb stops,
            string departureStopId, string arrivalStopId, DateTime departureTime) input)
        {
            var p = new Profile<TransferStats>(
                input.connections, input.stops,
                new InternalTransferGenerator(), new TransferStats(), TransferStats.ProfileTransferCompare);

            var depTime = DateTime.Now.Date.AddMinutes(10 * 60 + 25);

            // get departure and arrival stop ids.
            var reader = input.stops.GetReader();
            True(reader.MoveTo(input.departureStopId));
            var departure = reader.Id;
            True(reader.MoveTo(input.arrivalStopId));
            var arrival = reader.Id;

            // instantiate and run EAS.
            var pcs = new ProfiledConnectionScan<TransferStats>(
                departure, arrival,
                depTime, depTime.AddHours(2), p);
            var journeys = pcs.CalculateJourneys();

            // verify result.
            Assert.NotNull(journeys);
            True(journeys.Any());

            return journeys;
        }
    }
}