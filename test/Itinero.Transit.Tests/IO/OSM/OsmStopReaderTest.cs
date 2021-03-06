using Itinero.Transit.IO.OSM.Data;
using Xunit;

namespace Itinero.Transit.Tests.IO.OSM
{
    public class OsmStopReaderTest
    {
        [Fact]
        public void MoveTo_InvalidFormat_False()
        {
            var osmStopReader = new OsmStopsDb(0);
            var result = osmStopReader.TryGetId("https://totallyNotOsm.org/lat/qsdf", out _);
            Assert.False(result);
        }
    }
}