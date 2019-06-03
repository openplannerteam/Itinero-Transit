﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Itinero.Transit.Data;
using Itinero.Transit.Logging;
using Itinero.Transit.Tests.Functional.Algorithms;
using Itinero.Transit.Tests.Functional.Algorithms.CSA;
using Itinero.Transit.Tests.Functional.Algorithms.Search;
using Itinero.Transit.Tests.Functional.Data;
using Itinero.Transit.Tests.Functional.IO;
using Itinero.Transit.Tests.Functional.IO.LC;
using Itinero.Transit.Tests.Functional.IO.LC.Synchronization;
using Itinero.Transit.Tests.Functional.IO.OSM;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Log = Serilog.Log;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Local

// ReSharper disable PossibleMultipleEnumeration

namespace Itinero.Transit.Tests.Functional
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            EnableLogging();

            var testAll = args.Length > 0 && args[0].Equals("--full-test-suite");
            // testAll = true;
            if (testAll)
            {
                // These are all the tests, and will be run in full on the build server
                // Tests for devving are below this block
                LocalTests();
                try
                {

                    InternetTests();
                }
                catch (OperationCanceledException)
                {
                    Log.Warning("Some website is down... Skipping internet tests");
                }
                SlowTests();
                // ReSharper disable once RedundantJumpStatement
                return;
            }


            
            var tdb = new TransitDb();
            tdb.UseOsmRoute("https://www.openstreetmap.org/relation/9413958", DateTime.Now, DateTime.Now.AddHours(1));
            
            /*
            var input = TransitDb.ReadFrom(
                        new List<string>
                        {
                            TestAllAlgorithms._nmbs,
                            TestAllAlgorithms._osmCentrumShuttle,
                            TestAllAlgorithms._delijnOVl
                        }
                    ).SelectProfile(new DefaultProfile())
                    .SelectStops(TestAllAlgorithms.CoiseauKaaiOsm, TestAllAlgorithms.GentZwijnaardeDeLijn)
                    .SelectTimeFrame(TestAllAlgorithms.TestDate.AddHours(09),
                        TestAllAlgorithms.TestDate.AddHours(12))
                ;


            new EasLasComparison().Run(input);
            //*/
        }


        private static void LocalTests()
        {
            var db = new TestAllAlgorithms().ExecuteDefault();
            var nmbs = TransitDb.ReadFrom(TestAllAlgorithms._nmbs, 0);
            var wvl = TransitDb.ReadFrom(TestAllAlgorithms._delijnWvl, 1);

            new StopEnumerationTest().Run(new List<TransitDb>()
            {
                nmbs, wvl
            });

            new TripHeadsignTest().Run(db);

            TestClosestStopsAndRouting(db);
            Log.Information("Running NoDuplicationTest");

            new ConnectionsDbDepartureEnumeratorTest().Run(db);
            new DelayTest().Run(true);

            Log.Information("Running single TransitDb tests");
            new TestAllAlgorithms().ExecuteDefault();
            Log.Information("Running multi TransitDb tests");

            new TestAllAlgorithms().ExecuteMultiModal();
        }

        public static void InternetTests()
        {
            foreach (var r in OsmTest.TestRelations)
            {
                var t = new OsmTest();
                t.Run(r);
            }
            new NoDuplicationTest().Run();

            new RoutingTest().Run();
            new CachingTest().Run(true);
        }

        public static void SlowTests()
        {
            new MultipleLoadTest().Run(0);

            // This tests starts a timer which reloads a lot
            new TestAutoUpdating().Run(null);
        }


        private static void TestClosestStopsAndRouting(TransitDb db)
        {
            StopSearchTest.Default.RunPerformance((db, 4.336209297180176,
                50.83567623496864, 1000));
            StopSearchTest.Default.RunPerformance((db, 4.436824321746825,
                50.41119778957908, 1000));
            StopSearchTest.Default.RunPerformance((db, 3.329758644104004,
                50.99052927907061, 1000));
        }


        private static void EnableLogging()
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logFile = Path.Combine("logs", $"log-{date}.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(new JsonFormatter(), logFile)
                .WriteTo.Console()
                .CreateLogger();
#if DEBUG
            var loggingBlacklist = new HashSet<string>();
#else
            var loggingBlacklist = new HashSet<string>();
#endif
            Logger.LogAction = (o, level, message, parameters) =>
            {
                if (loggingBlacklist.Contains(o))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(o))
                {
                    message = $"[{o}] {message}";
                }

                if (level == TraceEventType.Verbose.ToString().ToLower())
                {
                    Log.Debug(message);
                }
                else if (level == TraceEventType.Information.ToString().ToLower())
                {
                    Log.Information(message);
                }
                else if (level == TraceEventType.Warning.ToString().ToLower())
                {
                    Log.Warning(message);
                }
                else if (level == TraceEventType.Critical.ToString().ToLower())
                {
                    Log.Fatal(message);
                }
                else if (level == TraceEventType.Error.ToString().ToLower())
                {
                    Log.Error(message);
                }
                else
                {
                    Log.Debug(message);
                }
            };
        }
    }
}