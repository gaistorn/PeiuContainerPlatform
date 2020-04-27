using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Hubbub.Model
{
    public class PushModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("interval")]
        public TimeSpan Interval { get; set; }

        [JsonProperty("items")]
        public List<PushItemModel> Items { get; set; }

        [JsonIgnore]
        public DateTime LastReadTime;
    }

    public class PushItemModel
    {
        [JsonProperty("qos")]
        public int Qos { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("template")]
        public JObject Template { get; set; }

       
    }
}
