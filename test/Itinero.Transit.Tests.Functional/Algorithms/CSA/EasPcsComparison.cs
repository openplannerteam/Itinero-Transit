using System;
using System.Linq;
using Itinero.Transit.Algorithms.CSA;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Walks;
using Itinero.Transit.Journeys;

namespace Itinero.Transit.Tests.Functional.Algorithms.CSA
{
    /// <summary>
    /// When running PCS (without pruning), the earliest route should equal the one calculated by EAS.
    /// If not  something is wrong
    /// </summary>
    public class EasPcsComparison : DefaultFunctionalTest
    {
        public static EasPcsComparison Default = new EasPcsComparison();

        protected override bool Execute(
            (TransitDb transitDb, string departureStopId, string arrivalStopId, DateTime
                departureTime, DateTime arrivalTime) input)
        {
            var latest = input.transitDb.Latest;
            var profile = new Profile<TransferMetric>(new InternalTransferGenerator(1),
                new CrowsFlightTransferGenerator(),
                TransferMetric.Factory, TransferMetric.ProfileTransferCompare);

            // get departure and arrival stop ids.
            var reader = latest.StopsDb.GetReader();
            True(reader.MoveTo(input.departureStopId));
            var departure = reader.Id;
            True(reader.MoveTo(input.arrivalStopId));
            var arrival = reader.Id;

            var eas = new EarliestConnectionScan<TransferMetric>
            (new ScanSettings<TransferMetric>(
                latest.Lst(),
                departure, arrival, input.departureTime, input.arrivalTime, profile));

            var easJ = eas.CalculateJourney();
            var pcs = new ProfiledConnectionScan<TransferMetric>(
                new ScanSettings<TransferMetric>(
                    latest.Lst(),
                    departure, arrival, input.departureTime, input.arrivalTime, profile));

            var pcsJs = pcs.CalculateJourneys();
            var pcsJ = pcsJs.Last();

            Information(easJ.ToString(latest));
            Information(pcsJ.ToString(latest));

            // PCS could find a route which arrives at the same time, but departs later
            True(easJ.Root.DepartureTime() <= pcsJ.Root.DepartureTime());
            True(easJ.ArrivalTime() <= pcsJ.ArrivalTime());

            return true;
        }
    }
}