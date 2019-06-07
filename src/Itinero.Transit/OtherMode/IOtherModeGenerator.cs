using System.Collections.Generic;
using Itinero.Transit.Data;

namespace Itinero.Transit.OtherMode
{
    /// <summary>
    /// The transfergenerator takes a journey and a next connection.
    /// Using those, it extends the journey if this is possible.
    /// </summary>
    public interface IOtherModeGenerator
    {
    
        /// <summary>
        /// Gives the time needed to travel from this stop to the next.
        /// This can be used to do time estimations.
        ///
        /// Returns Max_Value if not possible or if this is not the responsibility (e.g. for a walk, if from == to).
        ///
        /// Warning: the 'to'-IStop will often be the reader. Using 'reader.MoveTo' will thus often also change 'to', thus Lat&Lon should be read first
        /// </summary>
        /// <returns></returns>
        uint TimeBetween(IStopsReader reader, (double latitude, double longitude) from, IStop to);


        //// <summary>
        /// Gives the times needed to travel from this stop to all the given locations.
        /// This can be used to do time estimations.
        ///
        /// The target stop should not be included if travelling towards it is not possible.
        ///
        /// This method is used mainly for optimization.
        ///
        /// Warning: the enumerators in 'to' will often be a list of 'n' times the same object.
        /// However, calling 'MoveNext' will cause that object to change state.
        /// In other words, 'to' should always be used in a 'for-each' loop.
        /// </summary>
        /// <returns></returns>
        Dictionary<LocationId, uint> TimesBetween(IStopsReader reader, (double latitude, double longitude) from,
            IEnumerable<IStop> to);
        
        /// <summary>
        /// The maximum range of this IOtherModeGenerator, in meters.
        /// This generator will only be asked to generate transfers within this range.
        /// If an stop out of this range is given to create a transfer,
        /// the implementation can choose to either return a valid transfer or to return null
        /// </summary>
        /// <returns></returns>
        float Range();
    }
}