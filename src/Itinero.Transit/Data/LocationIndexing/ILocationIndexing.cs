using System.Collections.Generic;

namespace Itinero.Transit.Data.LocationIndexing
{
    public interface ILocationIndexing<T>
    {
        IEnumerable<T> GetInBox((double minlon, double maxlat) nw, (double maxlon, double minlat) se);
        List<T> GetInRange((double lon, double lat) valueTuple, double maxDistanceInMeter);

    }
}