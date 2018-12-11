using Itinero.Transit.Data;

namespace Itinero.Transit.Tests.Data
{
    
    /// <summary>
    /// Dummy implementation of ICOnnection
    /// </summary>
    public class Connection : IConnection
    {
        public Connection(uint id, ulong departureTime, ulong arrivalTime, uint tripId, (uint localTileId, uint localId) arrivalStop, (uint localTileId, uint localId) departureStop)
        {
            Id = id;
            DepartureTime = departureTime;
            ArrivalTime = arrivalTime;
            TravelTime = (ushort) (arrivalTime - departureTime);
            TripId = tripId;
            ArrivalStop = arrivalStop;
            DepartureStop = departureStop;
        }

        public uint Id { get; }

        public ulong ArrivalTime { get; }

        public ulong DepartureTime { get; }

        public ushort TravelTime { get; }

        public uint TripId { get; }

        public (uint localTileId, uint localId) DepartureStop { get; }

        public (uint localTileId, uint localId) ArrivalStop { get; }
    }
}