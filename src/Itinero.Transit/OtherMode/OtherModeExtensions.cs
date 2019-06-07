using System;
using System.Collections.Generic;
using Itinero.Transit.Data;
using Itinero.Transit.Journey;

namespace Itinero.Transit.OtherMode
{
    public static class OtherModeExtensions
    {
        public static IOtherModeGenerator UseCache(this IOtherModeGenerator fallback)
        {
            return new OtherModeCacher(fallback);
        }

        public static uint TimeBetween(this IOtherModeGenerator modeGenerator, IStopsReader reader, LocationId from,
            LocationId to)
        {
            reader.MoveTo(from);
            var lat = reader.Latitude;
            var lon = reader.Longitude;

            reader.MoveTo(to);
            return modeGenerator.TimeBetween(reader, (lat, lon), reader);
        }

        public static Dictionary<LocationId, uint> TimesBetween(this IOtherModeGenerator mode,
            IStopsReader reader, LocationId from,
            IEnumerable<IStop> to)
        {
            reader.MoveTo(from);
            var lat = reader.Latitude;
            var lon = reader.Longitude;
            return mode.TimesBetween(reader, (lat, lon), to);
        }

        /// <summary>
        /// Uses the otherMode to 'walk' towards all the reachable stops from the arrival of the given journey.
        /// The given journey will be extended to 'n' journeys.
        /// Returns an empty list if no other stop is in range
        /// </summary>
        public static IEnumerable<Journey<T>> WalkAwayFrom<T>(
            this Journey<T> journey,
            IOtherModeGenerator otherModeGenerator,
            IStopsReader stops) where T : IJourneyMetric<T>
        {
            var location = journey.Location;
            if (!stops.MoveTo(location))
            {
                throw new ArgumentException($"Location {location} not found, could not move to it");
            }


            var reachableLocations =
                stops.LocationsInRange(stops.Latitude, stops.Longitude, otherModeGenerator.Range());

            var times = otherModeGenerator.TimesBetween(stops, journey.Location, reachableLocations);

            foreach (var v in times)
            {
                var reachableLocation = v.Key;
                var time = v.Value;

                if (reachableLocation.Equals(location))
                {
                    continue;
                }

                var walkingJourney =
                    journey.ChainSpecial(Journey<T>.OTHERMODE, journey.Time + time, reachableLocation, TripId.Walk);

                yield return walkingJourney;
            }
        }

        public static IEnumerable<Journey<T>> WalkTowards<T>(
            this Journey<T> journey,
            IOtherModeGenerator otherModeGenerator,
            IStopsReader stops) where T : IJourneyMetric<T>
        {
            return new[] {journey}.WalkTowards(journey.Location, otherModeGenerator, stops);
        }

        /// <summary>
        /// Uses the otherMode to 'walk' from all the reachable stops from the departure of the given journey.
        /// The given 'n' journeys will be prefixed with a walk to the  'm' reachable locations, resulting in (at most) 'n*m'-journeys.
        /// Returns an empty list if no other stop is in range.
        ///
        /// IMPORTANT: All the journeys should have the same (given) Location
        /// </summary>
        public static IEnumerable<Journey<T>> WalkTowards<T>(
            this IEnumerable<Journey<T>> journeys,
            LocationId location,
            IOtherModeGenerator otherModeGenerator,
            IStopsReader stops) where T : IJourneyMetric<T>
        {
            if (!stops.MoveTo(location))
            {
                throw new ArgumentException($"Location {location} not found, could not move to it");
            }

            var reachableLocations =
                stops.LocationsInRange(stops.Latitude, stops.Longitude, otherModeGenerator.Range());

            var times = otherModeGenerator.TimesBetween(stops, location, reachableLocations);

            foreach (var j in journeys)
            {
                foreach (var v in times)
                {
                    var reachableLocation = v.Key;
                    var time = v.Value;

                    if (reachableLocation.Equals(location))
                    {
                        continue;
                    }

                    // Biggest difference: subtraction instead of addition
                    var walkingJourney =
                        j.ChainSpecial(Journey<T>.OTHERMODE, j.Time - time, reachableLocation, TripId.Walk);

                    yield return walkingJourney;
                }
            }
        }

        /// <summary>
        /// A very straightforward implementation to get multiple routings at the same time...
        /// 
        /// </summary>
        internal static Dictionary<LocationId, uint> DefaultTimesBetween(
            this IOtherModeGenerator modeGenerator, IStopsReader reader, (double lat, double lon) coorFrom,
            IEnumerable<IStop> to)
        {
            var times = new Dictionary<LocationId, uint>();
            foreach (var stop in to)
            {
                var time = modeGenerator.TimeBetween(reader, coorFrom, stop);
                if (time == uint.MaxValue)
                {
                    continue;
                }

                times.Add(stop.Id, time);
            }

            return times;
        }
    }
}