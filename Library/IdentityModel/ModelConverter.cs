using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model
{
    public static class ModelConverter
    {
        public static string ConvertToJsonString<TInput>(TInput input, params string[] IgnoreProperties)
        {
            string result = null;
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            if (IgnoreProperties.Length > 0)
            {
                jsonResolver.IgnoreProperty(typeof(TInput), IgnoreProperties);
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = jsonResolver;
                result = JsonConvert.SerializeObject(input, serializerSettings);
            }
            else
            {
                result = JsonConvert.SerializeObject(input);
            }
            return result;
        }

        public static TResult ConvertToJsonObject<TInput, TResult>(TInput input, params string[] IgnoreProperties)
        {

            var json = ConvertToJsonString<TInput>(input, IgnoreProperties);
            TResult result = JsonConvert.DeserializeObject<TResult>(json);
            return result;
        }

        public static JObject ConvertToJsonJObject<TInput>(TInput input, params string[] IgnoreProperties)
        {
            
            var json = ConvertToJsonString<TInput>(input, IgnoreProperties);
            JObject result = JObject.Parse(json);
            return result;
        }
    }
}
