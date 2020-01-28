using System;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Synchronization;
using Itinero.Transit.IO.LC;
using Itinero.Transit.Tests.Functional.Utils;
using Itinero.Transit.Utils;

namespace Itinero.Transit.Tests.Functional.Data
{
    public class NoDuplicationTest : FunctionalTest
    {  public override string Name => "No Duplication Test";

        private static int CountConnections(DateTime now, TransitDb tdb)
        {
            var latest = tdb.Latest;
            var count = 0;
            var enumerator = latest.Connections.GetEnumeratorAt(now.ToUnixTime());
            while (enumerator.MoveNext())
            {
                count++;
            }

            return count;
        }

        protected override void Execute()
        {
            var now = DateTime.Now.ToUniversalTime();

            var tdb = new TransitDb(0);
            var dataset = tdb.UseLinkedConnections(Belgium.SncbConnections, Belgium.SncbLocations,
                DateTime.MaxValue, DateTime.MinValue);
            var updater = new TransitDbUpdater(tdb, dataset.UpdateTimeFrame);

            updater.UpdateTimeFrame(now, now.AddMinutes(10));
            var totalConnections = CountConnections(now, updater.TransitDb);

            for (var i = 0; i < 10; i++)
            {
                updater.UpdateTimeFrame(now, now.AddMinutes(10));
                var newCount = CountConnections(now, updater.TransitDb);
                if (newCount > totalConnections * 1.1)
                {
                    throw new ArgumentException("Duplicates are building in the database");
                }
            }
        }
    }
}