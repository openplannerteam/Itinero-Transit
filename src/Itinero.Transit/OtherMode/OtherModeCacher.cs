using System.Collections.Generic;
using System.Linq;
using Itinero.Transit.Data;

namespace Itinero.Transit.OtherMode
{
    public class OtherModeCacher : IOtherModeGenerator
    {
        private IOtherModeGenerator _fallback;

        public OtherModeCacher(IOtherModeGenerator fallback)
        {
            _fallback = fallback;
        }


        private Dictionary<((double latitude, double longitude), LocationId tos), uint> _cacheSingle =
            new Dictionary<((double latitude, double longitude), LocationId tos), uint>();

        public uint TimeBetween(IStopsReader reader, (double latitude, double longitude) from, IStop to)
        {
            var key = (from, to.Id);
            if (_cacheSingle.ContainsKey(key))
            {
                return _cacheSingle[key];
            }

            var v = _fallback.TimeBetween(reader, @from, to);
            _cacheSingle[key] = v;
            return v;
        }


        private Dictionary<((double latitude, double longitude) from, List<LocationId> tos),
            Dictionary<LocationId, uint>> _cache =
            new Dictionary<((double latitude, double longitude) from, List<LocationId> tos),
                Dictionary<LocationId, uint>>();

        public Dictionary<LocationId, uint> TimesBetween(IStopsReader reader, (double latitude, double longitude) from,
            IEnumerable<IStop> to)
        {
            /**
             * Tricky situation ahead...
             *
             * The 'to'-list of IStops is probably generated with a return yield ala:
             * 
             *
             * var potentialStopsInRange = ...
             * for(var stop in potentialStopsInRange){
             *    if(distanceBetween(stop, target) <= range{
             *         reader.MoveTo(stop);
             *        yield return stop;
             *     }
             * }
             *
             * (e.g. SearchInBox, which uses 'yield return stopSearchEnumerator.Current')
             *
             * In other words, something as ToList would result in:
             * [reader, reader, reader, ... , reader], which points internally to the same stop
             *
             * But we want all the ids to be able to cache.
             * And we cant use the 'to'-list as cache key, because it points towards the same reader n-times.
             *
             *
             * TODO BUG
             */


            // FIRST select the IDs to make sure 'return yield'-enumerators run correctly on an enumerating object
            to = to.Select(stop => new Stop(stop)).ToList();
            var tos = to.Select(stop => stop.Id).ToList();
            var key = (from, tos);
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }

            var v = _fallback.TimesBetween(reader, @from, to);
            _cache[key] = v;
            return v;
        }

        public float Range()
        {
            return _fallback.Range();
        }
    }
}