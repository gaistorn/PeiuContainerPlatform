using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.App
{
    public class EventModel
    {
        public int SiteId { get; set; }
        public int DeviceType { get; set; }
        public int DeviceIndex { get; set; }
        public long UnixTimestamp { get; set; }
        public int FactoryCode { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EventStatus Status { get; set; }

        public string Name { get; set; }

        //public int FactoryCode { get; set; }
        public int GroupCode { get; set; }
        public UInt32 BitFlag { get; set; }
        public void SetTimestamp(DateTime dt)
        {
            UnixTimestamp = dt.ToFileTime();

        }
        public DateTime TimeStamp
        {
            get
            {
                return DateTime.FromFileTime(UnixTimestamp);
            }
        }

        public string GetUniqueKey() => $"{FactoryCode}{GroupCode}{DeviceType}{DeviceIndex}";

        public string GetTopicName()
        {
            return $"hubbub/{SiteId}/{DeviceType}/{DeviceIndex}/Event";
        }

        public bool Value { get; set; }

    }

    public enum EventStatus
    {
        New,
        Recovery
    }
}
