using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PEIU.DataServices;
using PES.Toolkit;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class MqttSubscribeWorker : DataSubscribeWorker, IHostedService
    {
        readonly MqttAddress mqttAddress;
        readonly ILogger<MqttSubscribeWorker> logger;
        const string PCS_SYSTEM = "PCS_SYSTEM";
        const string PCS_BMSINFO = "PCS_BMSINFO";
        const string PCS_BATTERY = "PCS_BATTERY";
        const string PV_SYSTEM = "PV_SYSTEM";
        readonly IDatabaseAsync db;

        public MqttSubscribeWorker(ILogger<MqttSubscribeWorker> _logger, MqttAddress _address, IRedisConnectionFactory _redis)
        {
            logger = _logger;
            mqttAddress = _address;
            db = _redis.Connection().GetDatabase();
        }

        protected override async Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            try
            {
                string data = Encoding.UTF8.GetString(payload);
                JObject jObj = JObject.Parse(data);
                int groupId = jObj["groupid"].Value<int>();
                int siteId = jObj["siteId"].Value<int>();
                string deviceId = jObj["normalizedeviceid"].Value<string>();
                string redisKey = CommonFactory.CreateRedisKey(siteId, groupId, deviceId);
                HashEntry[] hashValues = CreateHashEntry(jObj);
                await db.HashSetAsync(redisKey, hashValues);

                //if(groupId == 4) // PV일 경우
                //{
                //    float totalActivePower = jObj["EnergyTotalActivePower"].Value<float>();
                //    string sub_redis_key = CommonFactory.CreateRedisKey(siteId, groupId, "EnergyTotalActivePower");
                //    int deviceIndex = int.Parse(deviceId.TrimStart("PV".ToCharArray()));
                //    if(await db.KeyExistsAsync(sub_redis_key) == false)
                //    {
                //        await db.ListLeftPushAsync(sub_redis_key, new RedisValue[] { 0, 0, 0, 0 });
                //    }
                //    await db.ListSetByIndexAsync(sub_redis_key, deviceIndex - 1, totalActivePower);
                //}

            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Method: OnApplicationMessageReceived\n" + ex.Message);
            }
        }

        private HashEntry[] CreateHashEntry(JObject obj)
        {
            List<HashEntry> entry = new List<HashEntry>();
            foreach(var field in obj.Properties())
            {
                entry.Add(new HashEntry(field.Name, field.Value.ToString()));
            }
            return entry.ToArray();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.ConnectionAsync(mqttAddress.ClientId, mqttAddress.BindAddress, mqttAddress.Port, mqttAddress.QosLevel, mqttAddress.Topic);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }
    }
}
