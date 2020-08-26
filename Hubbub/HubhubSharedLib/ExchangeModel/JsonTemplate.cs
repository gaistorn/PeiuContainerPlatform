using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Hubbub
{
    public class JsonTemplate : ICloneable
    {
        [JsonProperty("qos")]
        public int Qos { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("template")]
        public JObject Template { get; set; }

        [JsonProperty("devicetype")]
        public int DeviceType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("category")]
        public DataCategory Category { get; set; }


        public static IEnumerable<JsonTemplate> OpenFileInFolder(string FolderName)
        {
            string FolderLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            string[] files = Directory.GetFiles(FolderLocation, "*.json");
            List<JsonTemplate> result = new List<JsonTemplate>();
            foreach (string file in files)
            {
                try
                {
                    JsonTemplate model = JsonConvert.DeserializeObject<JsonTemplate>(File.ReadAllText(file, Encoding.UTF8));
                    result.Add(model);
                }
                finally { }
            }
            return result;
        }

        public object Clone()
        {
            JsonTemplate NewClone = new JsonTemplate();
            PropertyInfo[] propertyInfos = typeof(JsonTemplate).GetProperties();
            foreach (PropertyInfo pi in propertyInfos)
            {
                if (pi.CanWrite && pi.CanRead)
                {
                    object value = pi.GetValue(this);
                    pi.SetValue(NewClone, value);
                }
            }
            return NewClone;
                    //pi.SetValue(NewClone, )
            //using (var ms = new MemoryStream())
            //{
            //    var formatter = new BinaryFormatter();
            //    formatter.Serialize(ms, this);
            //    ms.Position = 0;

            //    return (JsonTemplate)formatter.Deserialize(ms);
            //}
        }
    }
}
