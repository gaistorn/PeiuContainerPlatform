using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Power21.Device.Dao
{
    public class PushTemplate
    {
        [JsonProperty("templatefile")]
        public string TemplateFile { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("pushinterval")]
        public TimeSpan PushInterval { get; set; } = TimeSpan.FromSeconds(1);

        [JsonProperty("qoslevel")]
        public int QosLevel { get; set; } = (int)MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;

        [JsonIgnore]
        public JObject template;

        [JsonIgnore]
        public DateTime LastReadTime;
    }
}
