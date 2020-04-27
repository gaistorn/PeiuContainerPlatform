using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Power21.Device.Dao
{
    public class MapControl
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("point")]
        public MapPoint Point { get; set; }

        [JsonProperty("function")]
        public ushort FunctionCode { get; set; } = 3;

    }
}
