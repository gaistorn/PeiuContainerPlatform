using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    public class EventModel
    {
        public int SiteId { get; set; }
        public int DeviceType { get; set; }
        public int DeviceIndex { get; set; }
        public long UnixTimestamp { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventStatus Status { get; set; }

        public string Name { get; set; }

        public int FactoryCode { get; set; }
        public int GroupCode { get; set; }
        public ushort BitFlag { get; set; }
        //public DateTime TimeStamp
        //{
        //    get
        //    {

        //        return new DateTime(1970, 1, 1).AddSeconds(UnixTimestamp).ToLocalTime();
        //    }
        //}

        public bool Value { get; set; }

        public string GetTopicName()
        {
            return $"{HubbubInformation.GlobalHubbubInformation.topicroot}/{SiteId}/{DeviceType}/{DeviceIndex}/Event";
        }

    }

    public enum EventStatus
    {
        New,
        Recovery
    }
}
