using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Itinero.Transit.IO.LC.CSA.ConnectionProviders;
using Itinero.Transit.IO.LC.CSA.LocationProviders;
using Itinero.Transit.IO.LC.CSA.Utils;
// ReSharper disable MemberCanBePrivate.Global

[assembly: InternalsVisibleTo("Itinero.Transit.Tests")]
[assembly: InternalsVisibleTo("Itinero.Transit.Tests.Benchmarks")]

namespace Itinero.Transit.IO.LC.CSA
{
    public static class Belgium
    {
        public static Profile Sncb(LocalStorage storage, Downloader loader = null)
        {
            return new Profile(
                "SNCB",
                new Uri("https://graph.irail.be/sncb/connections"),
                new Uri("https://irail.be/stations"),
                storage,
                loader
            );
        }


        public static Profile DeLijn(LocalStorage storage, Downloader loader = null)
        {
            var profs = new List<Profile>
            {
                WestVlaanderen(storage, loader),
                OostVlaanderen(storage, loader),
                VlaamsBrabant(storage, loader),
                Limburg(storage, loader),
                Antwerpen(storage, loader)
            };

            var conn = new List<IConnectionsProvider>();
            var locations = new List<ILocationProvider>();
            foreach (var prof in profs)
            {
                conn.Add(prof);
                locations.Add(prof);
            }

            return new Profile(
                new ConnectionProviderMerger(conn),
                new LocationCombiner(locations));
        }


        private static Profile CreateDeLijnProfile(string province, LocalStorage storage,
            Downloader loader)
        {
            storage = storage.SubStorage("DeLijn");

            return new Profile(
                "DeLijnWvl",
                new Uri($"https://openplanner.ilabt.imec.be/delijn/{province}/connections"),
                new Uri($"https://openplanner.ilabt.imec.be/delijn/{province}/stops"),
                storage,
                loader
            );
        }

        public static Profile WestVlaanderen(LocalStorage storage, Downloader loader)
        {
            return CreateDeLijnProfile("West-Vlaanderen", storage, loader);
        }


        public static Profile OostVlaanderen(LocalStorage storage, Downloader loader)
        {
            return CreateDeLijnProfile("Oost-Vlaanderen", storage, loader);
        }


        public static Profile Limburg(LocalStorage storage, Downloader loader)
        {
            return CreateDeLijnProfile("Limburg", storage, loader);
        }


        public static Profile VlaamsBrabant(LocalStorage storage, Downloader loader)
        {
            return CreateDeLijnProfile("Vlaams-Brabant", storage, loader);
        }

        public static Profile Antwerpen(LocalStorage storage, Downloader loader)
        {
            return CreateDeLijnProfile("Antwerpen", storage, loader);
        }
    }
}