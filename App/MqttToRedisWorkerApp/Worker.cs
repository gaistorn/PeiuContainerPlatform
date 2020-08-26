using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PeiuPlatform.DataAccessor;
using StackExchange.Redis;

namespace MqttToRedisWorkerApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        readonly IRedisDataAccessor redisDataAccessor;
        readonly IDatabaseAsync db;
        readonly IPacketQueue packetQueue;
       // readonly UdpClient udpClient = new UdpClient(10000);

        public Worker(ILogger<Worker> logger, IRedisDataAccessor _redis, IPacketQueue queue)
        {
            _logger = logger;
            logger.LogInformation("entry worker");
            redisDataAccessor = _redis;
            db = _redis.Connection().GetDatabase();
            packetQueue = queue;
        }

       

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("execute worker");
            string target_ip = Environment.GetEnvironmentVariable("UDP_TARGET_IP");
            string target_port = Environment.GetEnvironmentVariable("UDP_TARGET_PORT");

            _logger.LogInformation("Target UDP Server: {0} : {1}", target_ip, target_port);
            int portNum = 11000;

            int.TryParse(target_port, out portNum);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    JObject jObj = await packetQueue.DequeueAsync(stoppingToken);
                    int groupId = jObj["groupid"].Value<int>();
                    int siteId = jObj["siteId"].Value<int>();
                    string deviceId = jObj["normalizedeviceid"].Value<string>();

                    //if (groupId == 4 && siteId == 6)
                    //{
                    //    PvPacket packet = ConvertPacket(jObj);
                    //    int packSize = packet.GetSize();
                    //    var pack = packet.ToByteArray();
                    //    int packLength = pack.Length;
                    //    int sendpackets = udpClient.Send(pack, packLength, target_ip, portNum);
                    //    _logger.LogInformation("SENDING PACKET ({0} bytes)", sendpackets);
                    //}


                    string redisKey = CreateRedisKey(siteId, groupId, deviceId);
                    HashEntry[] hashValues = CreateHashEntry(jObj);
                    await db.HashSetAsync(redisKey, hashValues);
                    await Task.Delay(100, stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        private string CreateRedisKey(int siteId, int groupId, params string[] childKeys)
        {
            string child = childKeys.Length > 0 ? "." + string.Join(".", childKeys) : "";
            return string.Format($"SID{siteId}.GID{groupId}{child}");
        }

        private PvPacket ConvertPacket(JObject obj)
        {
            PvPacket packet = new PvPacket();
            string deviceId = obj["deviceId"].Value<string>();
            int groupId = obj["groupid"].Value<int>();
            int siteId = obj["siteId"].Value<int>();
            bool injeju = obj["inJeju"].Value<int>() == 1;

            packet.deviceindex = ushort.Parse(deviceId.Substring(deviceId.Length - 1));
            packet.siteid = obj["siteId"].Value<ushort>();
            //packet.injeju = injeju;
            packet.rcc = 6;
            packet.totalactivepower = obj["TotalActivePower"].Value<float>();
            packet.totalreactivepower = obj["TotalReactivePower"].Value<float>();
            packet.reverseactivepower = obj["ReverseActivePower"].Value<float>();
            packet.reversereactivepower = obj["ReverseReactivePower"].Value<float>();

            packet.voltage = new rst();
            packet.voltage.r = obj["vltR"].Value<float>();
            packet.voltage.s = obj["vltS"].Value<float>();
            packet.voltage.t = obj["vltT"].Value<float>();

            packet.current = new rst();
            packet.current.r = obj["crtR"].Value<float>();
            packet.current.s = obj["crtS"].Value<float>();
            packet.current.t = obj["crtT"].Value<float>();

            packet.frequency = obj["Frequency"].Value<float>();
            packet.energytotalactivepower = obj["EnergyTotalActivePower"].Value<float>();
            packet.energytodayactivepower = obj["EnergyTodayActivePower"].Value<float>();
            packet.energytotalreactivepower = obj["EnergyTotalReactivePower"].Value<float>();
            packet.energytotalreverseactivepower = obj["EnergyTotalReverseActivePower"].Value<float>();
            return packet;

        }


        private HashEntry[] CreateHashEntry(JObject obj)
        {
            List<HashEntry> entry = new List<HashEntry>();
            foreach (var field in obj.Properties())
            {
                entry.Add(new HashEntry(field.Name, field.Value.ToString()));
            }
            return entry.ToArray();
        }
    }
}
