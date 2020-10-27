using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PeiuPlatform.Notification
{
    public class Notificator
    {
        public string API_KEY { get; set; }
        public NLog.ILogger Logger { get; set; }

        private const string API_BASE_URL = "https://api-sms.cloud.toast.com/";
        private string GetMMSUrl()=>
            $"sms/v2.3/appKeys/{API_KEY}/sender/mms";

        private readonly HttpClient client = new HttpClient();
        public Notificator(string APIKEY)
        {
            this.API_KEY = APIKEY;
            client.BaseAddress = new Uri(API_BASE_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<MessageResponse> SendingMMS(string Title, string Body, string SendNo, params string[] RecipentNo)
        {
            try
            {
                MMSRequestBodyDetail message = new MMSRequestBodyDetail(Title, Body, SendNo, RecipentNo);

                string url = GetMMSUrl();
                var respon = await client.PostAsJsonAsync<MMSRequestBodyDetail>(GetMMSUrl(), message);
                if (respon.IsSuccessStatusCode)
                {
                    MessageResponse result = await respon.Content.ReadAsAsync<MessageResponse>();
                    if(result.Header.IsSuccessful == false)
                    {
                        Logger.Error($"MMS Failed. Code: {result.Header.ResultCode} Reason: {result.Header.ResultMessage}");
                    }
                    return result;
                }
                else
                {
                    Logger.Error("Response Error:" + respon.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
            
            return null;
        }
    }
}
