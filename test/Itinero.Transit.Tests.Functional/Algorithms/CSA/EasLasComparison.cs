using Itinero.Transit.Journey.Metric;
using Itinero.Transit.Tests.Functional.Utils;
using Itinero.Transit.Utils;

namespace Itinero.Transit.Tests.Functional.Algorithms.CSA
{
    /// <summary>
    /// When running PCS (without pruning), the earliest route should equal the one calculated by EAS.
    /// If not  something is wrong
    /// </summary>
    public class EasLasComparison : FunctionalTestWithInput<WithTime<TransferMetric>>
    {
        protected override void Execute( )
        {
            var easJ =
                Input.CalculateEarliestArrivalJourney();

            NotNull(easJ);
            AssertNoLoops(easJ, Input);

            Input.ResetFilter();

            var lasJ =
                Input
                    .SelectTimeFrame(easJ.Root.DepartureTime().FromUnixTime(),
                       easJ.ArrivalTime().FromUnixTime())
                    .CalculateLatestDepartureJourney();


            NotNull(lasJ,
                $"No latest journey found for {Input.From[0].GlobalId} {Input.Start:s} --> {Input.From[1].GlobalId}. However, the earliest arrival journey has been found:" +
                $"\n{easJ.ToString(1, Input.StopsDb)}");
            AssertNoLoops(lasJ, Input);

            // Eas is bound by the first departing train, while las is not
            True(easJ.Root.DepartureTime() <= lasJ.Root.DepartureTime());
            True(easJ.ArrivalTime() == lasJ.ArrivalTime());
        }
    }
}