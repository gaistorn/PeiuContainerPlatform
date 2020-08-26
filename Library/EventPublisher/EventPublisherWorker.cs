using FireworksFramework.Mqtt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using LiteDB;
using PeiuPlatform.App;

namespace PeiuPlatform.Lib
{
    public class EventPublisherWorker : AbsMqttPublisher
    {
        public int FactoryCode { get;  }
        private int _siteId = 0;
        private string _deviceId = "0";
        private readonly string DBPath;
        

        public EventPublisherWorker(int FactoryCode)
        {
            this.FactoryCode = FactoryCode;
            DBPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            //DBFilePath = Path.Combine(DBPath, "eventdata.db");

        }

        protected override string GetMqttPublishTopicName()
        {
            //if (string.IsNullOrEmpty(DeviceId) || SiteId == -1)
            //    throw new Exception("DeviceId 또는 SiteId가 설정되지 않았습니다");
            string topic = $"hubbub/{_siteId}/{_deviceId}/Event";
            return topic;
        }

        public async Task UpdateDigitalPoint(int SiteId, int DeviceType, int DeviceIndex, int GroupCode, ushort BitValue, CancellationToken token)
        {

            try
            {
                await PublishEvent(SiteId, DeviceType, DeviceIndex, 2, GroupCode, BitValue, EventStatus.New, token);

            }
            catch(Exception ex)
            {

            }
        }

        //public async Task UpdateDigitalPoint(int SiteId, int DeviceType, int DeviceIndex, int GroupCode, ushort BitValue, CancellationToken token)
        //{

        //    try
        //    {
        //        string DeviceId = $"{DeviceType}{DeviceIndex}";
        //        string DBFilePath = Path.Combine(DBPath, $"{SiteId}_{DeviceId}.db");
        //        using (var db = new LiteDatabase(DBFilePath))
        //        {
        //            var value_lists = db.GetCollection("valuepoints");
        //            BsonValue key = CreateIdValue(GroupCode, FactoryCode);
        //            BsonDocument bsonValue = value_lists.FindById(key);

        //            if (bsonValue == null)
        //            {
        //                bsonValue = EventSchema.CreateEvent(key, FactoryCode, GroupCode, DeviceIndex, DeviceType, BitValue);
        //                value_lists.Insert(bsonValue);
        //            }
        //            else
        //            {
        //                BsonValue value = bsonValue[EventSchema.ValueProperty];
        //                if (value.AsInt32 == (int)BitValue)
        //                {
        //                    return;
        //                }
        //                else
        //                    value_lists.Update(bsonValue);
        //            }
        //            value_lists.Upsert(bsonValue);

        //            var active_events = GetDigitalMaps(this.FactoryCode, GroupCode, BitValue);
        //            var eventLists = db.GetCollection("currentevents");

        //            // 새로 발생된 이벤트 인지 체크
        //            foreach (EventSchema active_event in active_events)
        //            {
        //                BsonValue active_id = CreateIdValue(active_event);
        //                BsonDocument exist_event = eventLists.FindById(active_id);
        //                if (exist_event == null)
        //                {
        //                    eventLists.Insert(active_id, active_event.GetBsonDocument());
        //                    await PublishEvent(SiteId, DeviceType, DeviceIndex, active_event.FactoryCode, active_event.GroupCode, active_event.Value, EventStatus.New, token);

        //                }
        //            }

        //            // 복구된 이벤트 인지 체크
        //            foreach (BsonDocument db_event in eventLists.Find(x => x[EventSchema.FactoryCodeProperty].AsInt32 == this.FactoryCode &&
        //            x[EventSchema.GroupCodeProperty].AsInt32 == GroupCode))
        //            {
        //                if (active_events.Any(x => x.Value == db_event[EventSchema.ValueProperty].AsInt32) == false)
        //                {
        //                    eventLists.Delete(db_event[EventSchema.IdProperty]);
        //                    await PublishEvent(SiteId, DeviceType, DeviceIndex, db_event[EventSchema.FactoryCodeProperty].AsInt32, db_event[EventSchema.GroupCodeProperty].AsInt32, (ushort)db_event[EventSchema.ValueProperty].AsInt32, EventStatus.Recovery, token);
        //                }
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        private IEnumerable<EventSchema> GetDigitalMaps(int factoryCode, int groupCode, ushort BitValue)
        {
            if (BitValue == 0)
                return new EventSchema[] { };
            else
            {
                List<EventSchema> maps = new List<EventSchema>();
                for(ushort i=0; i<16;i++)
                {
                    int bit = (int)Math.Pow(2, i);
                    if ((BitValue & bit) == bit)
                    {
                        EventSchema map = new EventSchema() { Value = (ushort)bit, GroupCode = groupCode, FactoryCode = factoryCode };
                        maps.Add(map);
                    }
                }
                return maps;
            }
        }
        private BsonValue CreateIdValue(int GroupCode, int FactoryCode)
        {
            string key_Str = $"{GroupCode}{FactoryCode}";
            long key = long.Parse(key_Str);
            return new BsonValue(key);
        }


        private BsonValue CreateIdValue(int GroupCode, ushort BitFlag, int FactoryCode )
        {
            string key_Str = $"{GroupCode}{BitFlag}{FactoryCode}";
            long key = long.Parse(key_Str);
            return new BsonValue(key);
        }

        private BsonValue CreateIdValue(EventSchema map)
        {
            return CreateIdValue(map.GroupCode, map.Value, map.FactoryCode);
        }

        private async Task PublishEvent(int SiteId, int DeviceType, int DeviceIndex, int FactoryCode, int GroupCode, ushort Value, EventStatus status, CancellationToken token)
        {

            EventModel record = new EventModel();
            record.UnixTimestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds();
            record.DeviceType = DeviceType;
            record.DeviceIndex = DeviceIndex;
            this._deviceId = $"{DeviceType}/{DeviceIndex}";
            record.SiteId =  this._siteId = SiteId;
            record.Status = status;
            record.BitFlag = Value;
            record.GroupCode = GroupCode;
            string message = JsonConvert.SerializeObject(record);
            await base.PublishMessageAsync(message, token);
        }
    }
}
