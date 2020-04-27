using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Power21.Device.Dao
{
    public class Map
    {
        [JsonProperty("input")]
        public List<MapRow> Rows = new List<MapRow>();

        [JsonProperty("output")]
        public List<MapControl> Controls = new List<MapControl>();
    }
}
