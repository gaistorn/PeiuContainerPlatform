using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PeiuPlatform.DataAccessor;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransferJejuPv
{
    public class PeiuMqttSubscriber : DataSubscribeWorker
    {
        readonly ILogger logger;
        
        const string PCS_SYSTEM = "PCS_SYSTEM";
        const string PCS_BMSINFO = "PCS_BMSINFO";
        const string PCS_BATTERY = "PCS_BATTERY";
        const string PV_SYSTEM = "PV_SYSTEM";
        
        
        readonly string target_ip;
        readonly int target_port;
        readonly IPacketQueue packetQueue;

        public PeiuMqttSubscriber(ILogger _logger, IPacketQueue queue)
        {
            logger = _logger;
            packetQueue = queue;
           
        }

        protected override async Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            try
            {
                
                
                string data = Encoding.UTF8.GetString(payload);
                //logger.LogInformation(data);
                JObject jObj = JObject.Parse(data);
                int groupId = jObj["groupid"].Value<int>();
                if (groupId == 4)
                {
                    packetQueue.QueueBackgroundWorkItem(jObj);
                }

                await Task.Delay(200);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Method: OnApplicationMessageReceived\n" + ex.Message);
            }
        }

        

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
            await this.ConnectionAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }
    }
}
