using System.Collections.Generic;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Core;

namespace Itinero.Transit.OtherMode
{
    /// <summary>
    ///  Generates internal (thus within the station) transfers if there is enough time to make the transfer.
    /// Returns null if two different locations are given
    /// </summary>
    public class InternalTransferGenerator : IOtherModeGenerator
    {
        private readonly uint _internalTransferTime;

        public InternalTransferGenerator(uint internalTransferTime = 180)
        {
            _internalTransferTime = internalTransferTime;
        }


        public uint TimeBetween(IStop from, IStop to)
        {
            if (from.Id.Equals(to.Id))
            {
                return _internalTransferTime;
            }

            return uint.MaxValue;
        }

        public Dictionary<StopId, uint> TimesBetween(IStop from,
            IEnumerable<IStop> to)
        {
            // It is a tad weird to have this method implemented, as this one only works when from == to...
            // But well, here we go anyway
            return this.DefaultTimesBetween(from, to);
        }

        public Dictionary<StopId, uint> TimesBetween(IEnumerable<IStop> @from, IStop to)
        {
            return this.DefaultTimesBetween(from, to);
        }

        public uint Range()
        {
            return 0;
        }

        public string OtherModeIdentifier()
        {
            return "internalTransfer&timeNeeded=" + _internalTransferTime;
        }

        public IOtherModeGenerator GetSource(StopId @from, StopId to)
        {
            return this;
        }
    }
}