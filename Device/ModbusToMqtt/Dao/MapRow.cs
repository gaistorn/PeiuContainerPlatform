using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Power21.Device.Dao
{
    public class MapRow
    {
        [JsonProperty("function")]
        public ushort FunctionCode { get; set; }


        private List<MapPoint> _points = new List<MapPoint>();
        [JsonProperty("points")]
        public List<MapPoint> Points => _points;

        public ushort StartAddress()
        {
            return _points.Min(x => x.Offset);
        }

        public ushort NumOfPoints()
        {
            ushort lastOffset = _points.Max(x => x.Offset);
            return (ushort)(_points.FirstOrDefault(x => x.Offset == lastOffset).GetSize() + (lastOffset - StartAddress()));
        }
    }
}
