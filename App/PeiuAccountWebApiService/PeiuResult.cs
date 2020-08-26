using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class ApiResult
    {
        [JsonProperty("Result")]
        public Result Result { get; set; }
        [JsonProperty("Payload")]
        public Payload Payload { get; set; }

        public readonly static ApiResult OK_200 = new ApiResult() { Result = new Result() { Code = 200, Message = "success" } };
        public readonly static ApiResult BAD_REQUEST_400 = new ApiResult() { Result = new Result() { Code = 400, Message = "Bad request" } };

        public static ApiResult BadRequest(string Message)
        {
            return CreateResult(400, Message);
        }
        public static ApiResult CreateResult(int Code, string Message)
        {
            ApiResult result = new ApiResult() { Result = new Result() { Code = Code, Message = Message } };
            return result;
        }
        
        public static ApiResult Ok(IEnumerable<object> elements)
        {
            ApiResult result = new ApiResult() { Result = new Result() { Code = 200, Message = "success" } };
            result.Payload = new Payload();
            if(elements != null)
                result.Payload.Elements = new List<object>(elements);
            return result;
        }
    }

    public class Result
    {
        [JsonProperty("Code")]
        public int Code { get; set; }
        [JsonProperty("Message")]
        public string Message { get; set; }
    }

    public class Payload
    {
        public string responsetime { get; set; }
        public Payload()
        {
            this.responsetime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

        }
        public List<object> Elements { get; set; }
    }
}
