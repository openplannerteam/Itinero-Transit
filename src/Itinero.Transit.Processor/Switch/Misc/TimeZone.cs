using System;
using System.Collections.Generic;
using Itinero.Transit.Data;
using Itinero.Transit.Utils;

namespace Itinero.Transit.Processor.Switch.Misc
{
    internal class ShowTimeZone : DocumentedSwitch, IMultiTransitDbModifier, IMultiTransitDbSource, IMultiTransitDbSink
    {
        private static readonly string[] _names = {"--timezone"};

        private static string About =
            "Shows the current timezone of the machine.";


        private static readonly List<(List<string> args, bool isObligated, string comment, string defaultValue)>
            _extraParams =
                new List<(List<string> args, bool isObligated, string comment, string defaultValue)>
                {
                    SwitchesExtensions.opt("timezone",
                            "A timezone id to query information about, e.g. 'Europe/Brussels' (case sensitive). For a full reference, see [wikipedia](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones)")
                        .SetDefault(""),
                    SwitchesExtensions.opt("time", "A date to test parsing")
                        .SetDefault("now"),
                    SwitchesExtensions.opt("all", "If set, print all timezones installed on this machine")
                        .SetDefault("false")
                };

        private const bool IsStable = true;

        public ShowTimeZone() : base(_names, About, _extraParams, IsStable)
        {
        }

        private void Run(Dictionary<string, string> parameters)
        {

            if (TimeZoneInfo.GetSystemTimeZones().Count <= 50)
            {
                Console.WriteLine("WARNING: No or very little timezones were found.\n"+ParameterExtensions.TimezonesNotFoundMessage);
            }
            
            var t = parameters.ParseDate("time");
            var tzName = parameters["timezone"];
            Console.WriteLine($"The given time (in UTC) is {t.ToUniversalTime():s}");
            var tzinfo = TimeZoneInfo.Local;
            Console.WriteLine($"This is {t.ToLocalTime():s} in local time ({tzinfo.Id})");
            Console.WriteLine($"The UNIX-timestamp is {t.ToUniversalTime().ToUnixTime()}");
            Console.WriteLine(
                $"The timezone of this machine is {tzinfo.Id}, the offset is currently {tzinfo.BaseUtcOffset} ({tzinfo}). The offset can change during summer/wintertime");
            if (!string.IsNullOrEmpty(tzName))
            {
                tzinfo = TimeZoneInfo.FindSystemTimeZoneById(tzName);
                Console.WriteLine(
                    $"The requested timezone is {tzinfo.Id}, the offset is currently {tzinfo.BaseUtcOffset} ({tzinfo}). The offset can change during summer/wintertime");

                var tlocal = t.ConvertTo(tzinfo);
                Console.WriteLine(
                    $"The specified time is {tlocal:s}/{tzinfo.Id}");
                
            }

            if (parameters.ParseBool("all"))
            {
                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    Console.WriteLine(tz.Id);
                }
            }
        }


        public List<TransitDbSnapShot> Modify(Dictionary<string, string> parameters, List<TransitDbSnapShot> tdbs)
        {
            Run(parameters);
            return tdbs;
        }

        public List<TransitDbSnapShot> Generate(Dictionary<string, string> parameters)
        {
            Run(parameters);
            return new List<TransitDbSnapShot>();
        }

        public void Use(Dictionary<string, string> parameters, List<TransitDbSnapShot> transitDbs)
        {
            Run(parameters);
        }
    }
}