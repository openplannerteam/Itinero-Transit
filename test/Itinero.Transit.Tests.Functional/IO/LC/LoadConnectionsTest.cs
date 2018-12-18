using System;
using Itinero.Transit.IO.LC;
using Itinero.Transit.Data;
using Itinero.Transit.IO.LC.CSA;
using Itinero.Transit.IO.LC.CSA.Utils;

namespace Itinero.Transit.Tests.Functional.IO.LC
{
    /// <summary>
    /// Tests the load connections extension method.
    /// </summary>
    public class LoadConnectionsTest : FunctionalTest<(ConnectionsDb connections, StopsDb stops),
        (DateTime date, TimeSpan window)>
    {
        /// <summary>
        /// Gets the default location connections test.
        /// </summary>
        public static LoadConnectionsTest Default => new LoadConnectionsTest();
        
        protected override (ConnectionsDb connections, StopsDb stops) Execute((DateTime date, TimeSpan window) input)
        {
            // setup profile.
            var profile = Belgium.Sncb(new LocalStorage("cache"));

            // create a stops db and connections db.
            var stopsDb = new StopsDb();
            var tripsDb = new TripsDb();
            var connectionsDb = new ConnectionsDb();

            // load connections for the current day.
            connectionsDb.LoadConnections(profile, stopsDb, tripsDb, (input.date, input.window));

            return (connectionsDb, stopsDb);
        }
    }
}