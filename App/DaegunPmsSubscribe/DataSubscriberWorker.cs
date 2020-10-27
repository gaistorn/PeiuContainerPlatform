using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using PeiuPlatform.App;
using PeiuPlatform.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform
{

    public interface IPacketSubscriber
    {
        ConcurrentQueue<DataMessage> PublishQueues { get; }
        ConcurrentQueue<EventModel> PublishEventQueues { get; }
        void UpdateDigitalPoint(int SiteId, int DeviceType, int DeviceIndex, int FactoryCode, int GroupCode, uint BitValue, long dateTime);

        //IMqttClient MqttClient { get; }


    }
    public class DataSubscriberWorker : SubscribeWorker, IPacketSubscriber
    {
        readonly ILogger<DataSubscriberWorker> logger;
        private ConcurrentQueue<DataMessage> _queues;
        private ConcurrentQueue<EventModel> _eventQueues;
        const string ENV_MQTT_TOPIC = "ENV_MQTT_TOPIC";
        const string ENV_PEIU_CONTROL_TOPIC = "ENV_PEIU_CONTROL_TOPIC";
        const string ENV_PMS_CONTROL_TOPIC = "ENV_PMS_CONTROL_TOPIC";

        public DataSubscriberWorker(ILogger<DataSubscriberWorker> logger)
        {
            this.logger = logger;
            _queues = new ConcurrentQueue<DataMessage>();
            _eventQueues = new ConcurrentQueue<EventModel>();

            Initialize();


        }

        private async void Initialize()
        {
            string topic = Environment.GetEnvironmentVariable(ENV_MQTT_TOPIC);
            string ctl_topic = Environment.GetEnvironmentVariable(ENV_PEIU_CONTROL_TOPIC);
            base.Topics = new string[] { topic, ctl_topic };
            await base.ConnectionAsync();
        }

        public ConcurrentQueue<DataMessage> PublishQueues => _queues;
        public ConcurrentQueue<EventModel> PublishEventQueues => _eventQueues;
        public IMqttClient MqttClient => base.GetMqttClient();

        public void UpdateDigitalPoint(int SiteId, int DeviceType, int DeviceIndex, int FactoryCode, int GroupCode, uint BitValue, long timestamp)
        {
            string topicName = $"hubbub/{SiteId}/{DeviceType}/{DeviceIndex}/Event";
            EventModel record = new EventModel();
            record.UnixTimestamp = timestamp;
            record.DeviceType = DeviceType;
            record.DeviceIndex = DeviceIndex;
            record.SiteId = SiteId;
            record.FactoryCode = FactoryCode;
            record.Status = EventStatus.New;
            record.BitFlag = BitValue;
            record.GroupCode = GroupCode;
            _eventQueues.Enqueue(record);
        }

        private MqttApplicationMessage ConvertControlMessage(ModbusControlModel model)
        {
            DUT_MQTT_MANUALCOMMAND mc = new DUT_MQTT_MANUALCOMMAND();
            mc.Header = new DUT_MQTT_COMMON_HEADER();
            mc.Header.Quality = 1;
            mc.Header.Category = PmsCategoryTypes.MANUALCOMMAND;
            mc.Header.PmsIndex = 1;
            mc.Header.Timestamp = DateTime.Now.ToFileTime();
            mc.SequenceNumber = 1;
            mc.Device = 2;
            mc.SetUpFlag = 1;
            mc.UserName = model.userid;
            //mc.ReferenceValue = Convert.ToUInt32(model.commandvalue.Value);
            mc.Command = ParseCommandCode(model.commandcode, mc.ReferenceValue);
            if (model.commandvalue.HasValue)
                mc.ReferenceValue = Convert.ToUInt32(Math.Abs( model.commandvalue.Value));
            mc.DeviceIndex = (uint)model.deviceindex;

            string topic = Environment.GetEnvironmentVariable(ENV_PMS_CONTROL_TOPIC);
            byte[] packet = PacketParser.SturctToByte(mc);
            return new MqttApplicationMessageBuilder().
                WithExactlyOnceQoS().WithTopic(topic).WithPayload(packet).Build();

        }

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
                if (Topic.StartsWith("site"))
                {
                    DUT_MQTT_COMMON_HEADER header = PacketParser.ByteToStruct<DUT_MQTT_COMMON_HEADER>(payload);
                    switch(header.Category)
                    {
                        case PmsCategoryTypes.ESS:
                            DUT_MQTT_ESS ess = PacketParser.ByteToStruct<DUT_MQTT_ESS>(payload);
                            ModelConverter.DataProcessing(ess, _queues);
                            ModelConverter.EventProcessing(this, ess);
                            break;
                        case PmsCategoryTypes.TEMPERATUREANDHUMIDITY:
                            DUT_MQTT_TEMPHUMIDITY temp = PacketParser.ByteToStruct<DUT_MQTT_TEMPHUMIDITY>(payload);
                            ModelConverter.DataProcessing(temp, _queues);
                            break;
                    }
                    
                }
                else if(Topic.EndsWith("control"))
                {
                    string msg = Encoding.UTF8.GetString(payload);
                    ModbusControlModel model = JsonConvert.DeserializeObject<ModbusControlModel>(msg);
                    MqttApplicationMessage mqtt_msg = ConvertControlMessage(model);
                    await MqttClient.PublishAsync(mqtt_msg);
                    var logger = NLog.LogManager.GetLogger("control_logger");
                    logger.Info($"[{model.userid}] [DeviceIndex:{model.deviceindex}] {model.commandcode} REF:{model.commandvalue}");

                }

                //if(ModelConverter.TryConvertPcs(ess, DateTime.Now, out MqttApplicationMessage[] pcs_messages, 
                //    out MqttApplicationMessage[] bat_messages))
                //{ 

                //}

            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            //throw new NotImplementedException();
        }
    }
}
