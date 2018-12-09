﻿using System;
using System.Collections.Generic;
using Itinero.Transit.Data;
using Itinero.Transit.IO.LC.CSA.Stats;
using Itinero.Transit.IO.LC.CSA.ConnectionProviders;
using Itinero.Transit.Logging;

namespace Itinero.Transit.IO.LC
{
    /// <summary>
    /// Contains extensions methods related to the connections db.
    /// </summary>
    public static class ConnectionsDbExtensions
    {
        /// <summary>
        /// Loads connections into the connections db and the given stops db from the given profile.
        /// </summary>
        /// <param name="connectionsDb">The connections db.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="stopsDb">The stops db.</param>
        /// <param name="window">The window, a start time and duration.</param>
        /// <param name="countStart">(For testing): if you want to count the number of connections departing here (and arriving at countEnd), pass a paramater with the URI of the departure location</param>
        /// <param name="countEnd">See countStart</param>
        public static Dictionary<ulong, Uri> LoadConnections(this ConnectionsDb connectionsDb, 
            Itinero.Transit.IO.LC.CSA.Profile<TransferStats> profile,
            StopsDb stopsDb, (DateTime start, TimeSpan duration) window, out int count, string countStart = "", string countEnd = "")
        {
            var idToUri = new Dictionary<ulong, Uri>();
            var stopsDbReader = stopsDb.GetReader();

            var trips = new Dictionary<string, uint>();

            var connectionCount = 0;
            var stopCount = 0;
            var timeTable = profile.GetTimeTable(window.start);
            count = 0;
            do
            {
                foreach (var connection in timeTable.Connections())
                {
                    if (connection.DepartureLocation().ToString() == countStart &&
                        connection.ArrivalLocation().ToString() == countEnd)
                    {
                        count++;
                    }

                    var stop1Uri = connection.DepartureLocation();
                    var stop1Location = profile.GetCoordinateFor(stop1Uri);
                    if (stop1Location == null)
                    {
                        continue;
                    }
                    var stop1Id = stop1Uri.ToString();
                    (uint localTileId, uint localId) stop1InternalId;
                    if (!stopsDbReader.MoveTo(stop1Id))
                    {
                        stop1InternalId = stopsDb.Add(stop1Id, stop1Location.Lon, stop1Location.Lat);
                        stopCount++;
                    }
                    else
                    {
                        stop1InternalId = stopsDbReader.Id;
                    }

                    var stop2Uri = connection.ArrivalLocation();
                    var stop2Location = profile.GetCoordinateFor(stop2Uri);
                    if (stop2Location == null)
                    {
                        continue;
                    }
                    var stop2Id = stop2Uri.ToString();
                    (uint localTileId, uint localId) stop2InternalId;
                    if (!stopsDbReader.MoveTo(stop2Id))
                    {
                        stop2InternalId = stopsDb.Add(stop2Id, stop2Location.Lon, stop2Location.Lat);
                        stopCount++;
                    }
                    else
                    {
                        stop2InternalId = stopsDbReader.Id;
                    }

                    var tripUri = connection.Trip().ToString();
                    if (!trips.TryGetValue(tripUri, out var tripId))
                    {
                        tripId = (uint) trips.Count;
                        trips[tripUri] = tripId;

                        Log.Information($"Added new trip {tripUri} with {tripId}");
                    }

                    var connectionId = connection.Id().ToString();
                    connectionsDb.Add(stop1InternalId, stop2InternalId, connectionId,
                        connection.DepartureTime(),
                        (ushort) (connection.ArrivalTime() - connection.DepartureTime()).TotalSeconds, tripId);
                    connectionCount++;
                }

                if (timeTable.NextTableTime() > window.start + window.duration)
                {
                    break;
                }

                var nextTimeTableUri = timeTable.NextTable();
                timeTable = profile.GetTimeTable(nextTimeTableUri);
            } while (true);

            Log.Information($"Added {stopCount} stops and {connectionCount} connection.");
            return idToUri;
        }
    }
}