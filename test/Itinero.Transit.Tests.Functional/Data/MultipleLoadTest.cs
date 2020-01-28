using System;
using Itinero.Transit.Data;
using Itinero.Transit.Data.Synchronization;
using Itinero.Transit.Tests.Functional.Utils;
using Itinero.Transit.Utils;

namespace Itinero.Transit.Tests.Functional.Data
{
    public class MultipleLoadTest : FunctionalTest
    {
        public override string Name => "Multiple Load Test";

        protected override void Execute()
        {
            var sncb = Belgium.Sncb();

            void UpdateTimeFrame(IWriter w, DateTime start, DateTime end)
            {
                sncb.AddAllConnectionsTo(w, start, end);
            }

            var db = new TransitDb(0);
            var dbUpdater = new TransitDbUpdater(db, UpdateTimeFrame);

            var writer = db.GetWriter();
            sncb.AddAllLocationsTo(writer);
            db.CloseWriter();

            var hours = 1;

            dbUpdater.UpdateTimeFrame(DateTime.Today.ToUniversalTime(),
                DateTime.Today.AddHours(hours).ToUniversalTime());
            Test(db);

            dbUpdater.UpdateTimeFrame(DateTime.Today.AddDays(1).ToUniversalTime(),
                DateTime.Today.AddDays(1).AddHours(hours).ToUniversalTime());
            Test(db);

            dbUpdater.UpdateTimeFrame(DateTime.Today.AddHours(-hours).ToUniversalTime(),
                DateTime.Today.AddHours(0).ToUniversalTime());
            Test(db);
        }

        private void Test(TransitDb db)
        {
            var conns = db.Latest.Connections;

            var count = 0;

            var enumerator = conns.GetEnumeratorAt(DateTime.Today.AddHours(10).ToUniversalTime().ToUnixTime());
            var endTime = DateTime.Today.AddHours(11).ToUniversalTime().ToUnixTime();
            while (enumerator.MoveNext() && enumerator.CurrentTime < endTime)
            {
                count++;
            }

            True(count > 0);
            new TripHeadsignTest().Run(db);
        }
    }
}