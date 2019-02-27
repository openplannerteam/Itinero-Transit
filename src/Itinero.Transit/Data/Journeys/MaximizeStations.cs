using System.Collections.Generic;

namespace Itinero.Transit.Journeys
{
    public partial class TravellingTimeMinimizer
    {
        public partial class Minimizer
        {
            /// <summary>
            /// A simple Journey Comparer, which walks along two journeys and takes the difference in station importance.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class MaximizeStations<T> : Comparer<Journey<T>> where T : IJourneyStats<T>
            {
                
                private readonly Dictionary<(uint, uint), uint> _importances;

                public MaximizeStations(Dictionary<(uint, uint), uint> importances)
                {
                    _importances = importances;
                }


                public override int Compare(Journey<T> x, Journey<T> y)
                {
                    var sum = 0;
                    if (x.PreviousLink != null && y.PreviousLink != null)
                    {
                        sum += Compare(x.PreviousLink, y.PreviousLink);
                    }

                    uint xL = 0;
                    uint yL = 0;
                    _importances.TryGetValue(x.Location, out xL);
                    _importances.TryGetValue(y.Location, out yL);


                    sum += (int) (yL - xL);
                    return sum;
                }
            }
        }
    }
}