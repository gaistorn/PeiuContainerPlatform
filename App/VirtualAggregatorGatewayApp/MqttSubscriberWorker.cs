using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using PeiuPlatform.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform
{

    public class MqttSubscriberWorker : SubscribeWorker
    {
        readonly ILogger<MqttSubscriberWorker> logger;
        readonly IGlobalStorage globalStorage;
        const string ENV_PEIU_CONTROL_TOPIC = "ENV_PEIU_CONTROL_TOPIC";

        public MqttSubscriberWorker(ILogger<MqttSubscriberWorker> logger, IGlobalStorage globalStorage)
        {
            this.logger = logger;
            this.globalStorage = globalStorage;
            Initialize();
        }

        private async void Initialize()
        {
            string ctl_topic = Environment.GetEnvironmentVariable(ENV_PEIU_CONTROL_TOPIC);
            base.Topics = new string[] { ctl_topic };
            await base.ConnectionAsync();
        }

        public IMqttClient MqttClient => base.GetMqttClient();

        private uint ParseCommandCode(ModbusCommandCodes code, UInt32 refvalue)
        {
            switch(code)
            {
                case ModbusCommandCodes.STOP:
                    return 1;
                case ModbusCommandCodes.STANDBY:
                    return 2;
                case ModbusCommandCodes.CHARGE:
                    return 3;
                case ModbusCommandCodes.DISCHARGE:
                    return 4;
                case ModbusCommandCodes.EMERGENCY_STOP:
                    return 5;
                case ModbusCommandCodes.RESET:
                    return 6;
                case ModbusCommandCodes.RELAY_CLOSE:
                    return 7;
                case ModbusCommandCodes.RELAY_OPEN:
                    return 8;
                case ModbusCommandCodes.ACTIVE_POWER:
                    if (refvalue > 0)
                        return 4;
                    else
                        return 3;
                default:
                    return 0;
            }
        }

        protected override async Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            try
            {
                string msg = Encoding.UTF8.GetString(payload);
                ModbusControlModel model = JsonConvert.DeserializeObject<ModbusControlModel>(msg);
                globalStorage.ControlModelQueues.Enqueue(model);
                var logger = NLog.LogManager.GetLogger("control_logger");
                logger.Info($"[{model.userid}] [DeviceIndex:{model.deviceindex}] {model.commandcode} REF:{model.commandvalue}");

            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            //throw new NotImplementedException();
        }

        //public async Task StartAsync(CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            int siteid = int.Parse(Environment.GetEnvironmentVariable("ENV_SITE_ID"));
            int rcc = int.Parse(Environment.GetEnvironmentVariable("ENV_RCC_ID"));

            while (!stoppingToken.IsCancellationRequested)
            {
                if(globalStorage.DataMessageQueues.TryDequeue(out DataMessage result))
                {
                    await MqttClient.PublishAsync(result.Message);
                }

                await Task.Delay(10);
            }
        }
    }
}
