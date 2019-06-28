using System;
using System.Collections.Generic;
using Itinero.Transit.Journey;
using Itinero.Transit.Journey.Filter;
using Itinero.Transit.Logging;

namespace Itinero.Transit.Algorithms.CSA
{
    /// <summary>
    /// A Pareto frontier is a collection of elements so that every element in the frontier outperforms the others on
    /// at least one metric.
    ///
    /// This Pareto-Frontier is also 'time-aware' in the sense that - if a journey departs earlier but arrives earlier -
    /// it is considered as optimal as well.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProfiledParetoFrontier<T>
        where T : IJourneyMetric<T>

    {
        public readonly MetricComparator<T> Comparator;
        public readonly IJourneyFilter<T> JourneyFilter;

        /// <summary>
        /// Contains all the points on the frontier.
        /// Although the list will often be sorted by descending Journey.Time (thus the earliest **departure** is last in the list) - this is not always the case!
        /// This is needed for certain optimisations.
        /// Note that most removals (if they happen) will probably be on the tail, so not have too much of an performance impact
        /// </summary>
        public readonly List<Journey<T>> Frontier = new List<Journey<T>>();

        public ProfiledParetoFrontier(MetricComparator<T> comparator, IJourneyFilter<T> journeyFilter)
        {
            Comparator = comparator ?? throw new ArgumentNullException(nameof(comparator),
                             "A Pareto Frontier can not operate without comparator");
            JourneyFilter = journeyFilter;
        }


        /// <summary>
        /// If the given journey is pareto-optimal in respect to the current frontier,
        /// the journey is added.
        /// If this journey outperforms some other point on the frontier, that point is removed
        /// </summary>
        /// <param name="considered"></param>
        /// <returns>True if the journey was appended to the frontier</returns>
        public bool AddToFrontier(Journey<T> considered)
        {
            if (considered == null || ReferenceEquals(considered, Journey<T>.InfiniteJourney))
            {
                return false;
            }

            if (JourneyFilter != null && !JourneyFilter.CanBeTakenBackwards(considered))
            {
                return false;
            }

            if (considered.Root.Time < considered.Time)
            {
                // This is not a backward journey
                throw new Exception("Not a backwards journey in the Pareto Frontier");
            }

            /*
             * PCS runs backwards, thus starts at the latest departing journeys
             * This means that journeys which are added, will probably depart earlier and that
             * Frontier is sorted on Journey.Time, with the lowest (earliest) times to the end.
             * However, in a very few cases this order might be disturbed (mostly footpaths) because a footpath migth generate a walk
             * which is longer then another walk and arrive before another train is inserted.
             *
             * Thus, this sadly breaks the sorting.
             * This might be fixed in the future though through an addition queueu or smthng similar
             */


            for (var i = Frontier.Count - 1; i >= 0; i--)
            {
                var guard = Frontier[i];

                // First thing to check:
                // Does one completely overlap the other?


                if (
                    considered.Time <= guard.Time && guard.Root.Time <= considered.Root.Time)
                {
                    // Guard completely falls within considered

                    // It might kill considered
                    var duel = Comparator.ADominatesB(guard, considered);

                    switch (duel)
                    {
                        case -1:
                            // The new journey didn't make the cut
                            return false;
                        case 0: // Both have equally good stats
                            // Notice that the guard completely falls within considered - but not strictly
                            // That means that considered might equal guard (or their timings could be the same at least
                            if (considered.Time == guard.Time && guard.Root.Time == considered.Root.Time)
                            {
                                // Identical timings and identical stats... Did we perhaps end up with the same journey?
                                if (considered.Equals(guard))
                                {
                                    return false;
                                }
                                // Both journeys are not identical, but parts of a family of closely related solutions
                                // We merge the journeys into one object to optimize

                                Frontier[i] = new Journey<T>(guard, considered);
                                return true;
                            }
                            else
                            {
                                // Both have equally good stats... But the guard strictly falls within considered
                                // Thus: considered takes a longer time and is worse
                                return false;
                            }

                        case 1:
                            // The new journey defeated the guard...
                            // Is is still an element of this frontier?
                            if (considered.Time == guard.Time && guard.Root.Time == considered.Root.Time)
                            {
                                // The new journey has the same time window ánd is better... Down with the guard
                                Frontier.RemoveAt(i);
                                i--;
                            }

                            // The guard is still better on the time aspect then the considered journey
                            // NO action is taken
                            break;

                        case int.MaxValue:
                            // Both are better then the other on some different statistic
                            // So: 1) The guard can not eliminate the candidate
                            // 2) The candidate can not eliminate the guard 
                            // We just have to continue scanning - if no guard defeats the candidate, it owned its place
                            continue;
                    }
                }
                else if (guard.Time <= considered.Time && considered.Root.Time <= guard.Root.Time)
                {
                    // Considered completely and strictly falls within guard
                    // It might kill the guard

                    var duel = Comparator.ADominatesB(guard, considered);
                    switch (duel)
                    {
                        case int.MaxValue: // Both are better then the other on some different dimension
                        /* Fallthrough to -1 */
                        case -1:
                            // The new journey is worse on some aspect then the guard...
                            // Except that it is faster!

                            // So: 1) The guard can not eliminate the candidate
                            // 2) The candidate can not eliminate the guard 

                            // No conclusion can be drawn
                            // We just have to continue scanning - if no guard defeats the candidate, it owned its place

                            continue;
                        case 0:
                        // The new journey is strictly faster and is just as good on other aspects as the guard
                        // That makes the new journey strictly better! Down with the guard:
                        /*Fallthrough to case 1*/
                        case 1:
                            // The new journey defeated the guard _and_ it is faster... Down with the guard!
                            Frontier.RemoveAt(i);
                            i--;
                            continue; // We continue the loop to remove other, possible sub-optimal entries further ahead in the list

                        default:
                            throw new Exception("Comparison of two journeys in metric did not return -1,1 or 0 but " +
                                                duel);
                    }
                }
                else
                {
                    // No comparison is possible - no need for a duel
                    continue;
                }
            }

            Frontier.Add(considered);


            return true;
        }


        internal void IsSorted()
        {
            var lastDep = Frontier[0];
            foreach (var journey in Frontier)
            {
                if (lastDep.Time >= journey.Time)
                {
                    lastDep = journey;
                }
                else
                {
                    throw new Exception("Not sorted. A journey departs earlier then its predecessor");
                }
            }
        }


        /// <summary>
        /// Considers all of the journeys to append them to the frontier.
        /// Returns all journeys which were added to the frontier
        ///
        /// IMPORTANT: Make sure to consume the iterator! Otherwise the 'yield returns' won't execute everything
        /// 
        /// </summary>
        public IEnumerable<Journey<T>> AddAllToFrontier(IEnumerable<Journey<T>> journeys)
        {
            foreach (var journey in journeys)
            {
                var wasAdded = AddToFrontier(journey);
                if (wasAdded)
                {
                    yield return journey;
                }
            }
        }


        public override string ToString()
        {
            var result = $"Pareto frontier with {Frontier.Count} entries";

            foreach (var j in Frontier)
            {
                result += "\n" + j;
            }

            return result;
        }
    }
}