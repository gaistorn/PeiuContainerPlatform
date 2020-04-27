using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Criterion;
using PeiuPlatform.DataAccessor;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace PeiuPlatform.App
{
    public class EventSubscribeWorker : DataSubscribeWorker
    {
        readonly EventDataAccessor eventDataAccessor;
        readonly IDatabaseAsync database;
        readonly ILogger<EventSubscribeWorker> logger;
        public EventSubscribeWorker(ILogger<EventSubscribeWorker> _logger, IRedisDataAccessor _redis, EventDataAccessor eventDataAccessor)
        {
            this.eventDataAccessor = eventDataAccessor;
            database = _redis.GetDatabase();
            logger = _logger;
            
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

        private async Task<IList<EventMap>> GetEventMaps(ISession session, EventModel data)
        {
            var map = await session.CreateCriteria<EventMap>()
                               .Add(Restrictions.Eq("Factorycode", data.FactoryCode)
                               && Restrictions.Eq("Groupcode", data.GroupCode)
                               && Restrictions.Eq("Devicetype", data.DeviceType))
                               .ListAsync<EventMap>();
            return map;
        }

        private async Task<EventMap> GetEventMap(ISession session, EventModel data, int bitValue, bool CreateMapWhenNotFound = true)
        {
            EventMap map = await session.CreateCriteria<EventMap>()
                               .Add(Restrictions.Eq("Factorycode", data.FactoryCode)
                               && Restrictions.Eq("Groupcode", data.GroupCode)
                               && Restrictions.Eq("Bitflag", bitValue)
                               && Restrictions.Eq("Devicetype", data.DeviceType))
                               .UniqueResultAsync<EventMap>();
            if (map == null && CreateMapWhenNotFound)
            {
                logger.LogWarning($"다음과 같은 이벤트를 못찾음. FC:{data.FactoryCode} / GC:{data.GroupCode} / BF:{bitValue} / DT:{data.DeviceType}");
                map = new EventMap();
                map.Factorycode = data.FactoryCode;
                map.Groupcode = data.GroupCode;
                map.Bitflag = bitValue;
                map.Devicetype = data.DeviceType;
                map.Level = 0;
                map.Name = $"Unknown(0x{bitValue.ToString("X")})";
                await session.SaveOrUpdateAsync(map);
            }
            return map;
        }

        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, ushort>>>> evtList = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, ushort>>>>();

        private void SetLatestBitValue(EventModel data, ushort value)
        {
            if (evtList.ContainsKey(data.SiteId) == false)
                evtList.Add(data.SiteId, new Dictionary<int, Dictionary<int, Dictionary<int, ushort>>>());
            if(evtList[data.SiteId].ContainsKey(data.FactoryCode) == false)
                evtList[data.SiteId].Add(data.FactoryCode, new Dictionary<int, Dictionary<int, ushort>>());
            if (evtList[data.SiteId][data.FactoryCode].ContainsKey(data.GroupCode) == false)
                evtList[data.SiteId][data.FactoryCode].Add(data.GroupCode, new Dictionary<int, ushort>());
            if (evtList[data.SiteId][data.FactoryCode][data.GroupCode].ContainsKey(data.DeviceType) == false)
                evtList[data.SiteId][data.FactoryCode][data.GroupCode].Add(data.DeviceType, value);
            else
                evtList[data.SiteId][data.FactoryCode][data.GroupCode][data.DeviceType] = value;
        }

        private ushort GetLatestBitValue(EventModel data)
        {
            ushort bitValue = ushort.MaxValue;
            if (evtList.ContainsKey(data.SiteId) &&
                evtList[data.SiteId].ContainsKey(data.FactoryCode) &&
                evtList[data.SiteId][data.FactoryCode].ContainsKey(data.GroupCode) &&
                evtList[data.SiteId][data.FactoryCode][data.GroupCode].ContainsKey(data.DeviceType))
            {
                bitValue = evtList[data.SiteId][data.FactoryCode][data.GroupCode][data.DeviceType];
            }
            return bitValue;
            
        }

        protected override async Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload)
        {
            try
            {
                byte[] packet = payload;
                string txt = Encoding.UTF8.GetString(packet);
                //logger.LogInformation($"Received event: " + txt);

                EventModel data = JsonConvert.DeserializeObject<EventModel>(txt);
                if (data == null)
                    return;
                DateTime dt = new DateTime(1970, 1, 1).AddSeconds(data.UnixTimestamp).AddHours(9);
                using (var session = eventDataAccessor.SessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    ushort latestBitValue = GetLatestBitValue(data);
                    if(latestBitValue == data.BitFlag)
                    {
                        return;
                    }
                    else
                    {
                        SetLatestBitValue(data, data.BitFlag);
                    }
                    if (data.BitFlag == 0)
                    {
                        IList<EventMap> evtList = await GetEventMaps(session, data);
                        if(evtList.Count > 0)
                        {
                            var existEvents = await session.CreateCriteria<EventRecord>().Add(
                               Restrictions.InG<int>("Eventcode", evtList.Select(x=>x.Eventcode)) && Restrictions.IsNull("Recoveryts") && Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
                            bool HasChanged = false;
                            foreach (EventRecord record in existEvents)
                            {
                                record.Recoveryts = dt;
                                logger.LogWarning($"[RECOVERY EVENT] SITEID: {data.SiteId} / FC:{data.FactoryCode} / GC:{data.GroupCode} / BF:{data.BitFlag} / DT:{data.DeviceType}");
                                HasChanged = true;
                                await session.UpdateAsync(record);
                            }
                            if (HasChanged)
                            {
                                await transaction.CommitAsync();
                            }
                            return;
                        }
                    }
                    
                    for (ushort i = 0; i < 16; i++)
                    {
                        int bit = (int)Math.Pow(2, i);
                       
                        if (bit > data.BitFlag)
                            break;
                        if ((data.BitFlag & bit) == bit)
                        {
                            // 발생한 이벤트가 이미 존재하는지 체크
                            EventMap map = await GetEventMap(session, data, bit);
                            // 복구 이벤트 체크

                            var existEvents = await session.CreateCriteria<EventRecord>().Add(
                                Restrictions.Eq("Eventcode", map.Eventcode) && Restrictions.IsNull("Recoveryts") && Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
                            // 이벤트가 발생했는데, 기존에 엑티브 이벤트에 없을 경우 이벤트를 인서트 한다.
                            if (existEvents.Count == 0)
                            {
                                EventRecord newEventRecode = new EventRecord();
                                newEventRecode.Createts = dt;
                                newEventRecode.Eventcode = map.Eventcode;
                                newEventRecode.Siteid = data.SiteId;
                                newEventRecode.Devicetype = data.DeviceType;
                                newEventRecode.Deviceindex = data.DeviceIndex;
                                await session.SaveOrUpdateAsync(newEventRecode);
                                logger.LogWarning($"[NEW EVENT] SITEID: {data.SiteId} / FC:{data.FactoryCode} / GC:{data.GroupCode} / BF:{bit} / DT:{data.DeviceType}");
                            }
                        }
                        else
                        {
                            // 복구가 된 경우
                            EventMap map = await GetEventMap(session, data, bit, false);
                            if (map == null)    // 없는 이벤트 일 경우 무시
                                continue;
                            var existEvents = await session.CreateCriteria<EventRecord>().Add(
                                Restrictions.Eq("Eventcode", map.Eventcode) && Restrictions.IsNull("Recoveryts") && Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
                            foreach(EventRecord record in existEvents)
                            {
                                record.Recoveryts = dt;
                                await session.UpdateAsync(record);
                                logger.LogWarning($"[RECOVERY EVENT] SITEID: {data.SiteId} / FC:{data.FactoryCode} / GC:{data.GroupCode} / BF:{data.BitFlag} / DT:{data.DeviceType}");
                            }

                        }
                    }
                    //EventMap map = await session.CreateCriteria<EventMap>()
                    //   .Add(Restrictions.Eq("Factorycode", data.FactoryCode) 
                    //   && Restrictions.Eq("Groupcode", data.GroupCode)
                    //   && Restrictions.Eq("Bitflag", (int)data.BitFlag)
                    //   && Restrictions.Eq("Devicetype", data.DeviceType))
                    //   .UniqueResultAsync<EventMap>();
                    //string redisKey = $"SID{data.SiteId}.DI.{data.DeviceType}.{data.DeviceIndex}";
                    //if(map == null)
                    //{
                    //    logger.LogWarning($"다음과 같은 이벤트를 못찾음. FC:{data.FactoryCode} / GC:{data.GroupCode} / BF:{data.BitFlag} / DT:{data.DeviceType}");
                    //    map = new EventMap();
                    //    map.Factorycode = data.FactoryCode;
                    //    map.Groupcode = data.GroupCode;
                    //    map.Bitflag = data.BitFlag;
                    //    map.Devicetype = data.DeviceType;
                    //    map.Level = 0;
                    //    map.Name = "Unknown";
                    //    await session.InsertAsync(map);
                    //}
                    //EventRecord s;
                    ////s.Siteid
                    //// 복구 이벤트 체크

                    //var existEvents = await session.CreateCriteria<EventRecord>().Add(
                    //    Restrictions.Eq("Eventcode", map.Eventcode) && Restrictions.IsNull("Recoveryts") && Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
                    //foreach(EventRecord evt in existEvents)
                    //{
                    //    bool IsRecovery = 

                    //}

                    //if (await database.HashExistsAsync(redisKey, map.Eventcode) == false)
                    //{
                    //    await database.HashSetAsync(redisKey, map.Eventcode, bool.FalseString);
                    //}

                    

                    //if (data.Status == EventStatus.New)
                    //{
                        

                    //    if (map == null)
                    //    {
                    //        logger.LogWarning($"서버에 존재하지 않는 로그 이벤트 발생. -> {txt}");
                    //        return;
                    //    }

                    //    await database.HashSetAsync(redisKey, map.Eventcode, bool.TrueString);
                    //    var eventFromRecord = await session.CreateCriteria<EventRecord>()
                    //        //.Add(Restrictions.IsNull("RecoveryDT"))
                    //        .Add(Restrictions.Eq("Eventcode", map.Eventcode) && Restrictions.Eq("Siteid", data.SiteId)
                    //        && Restrictions.Eq("Devicetype", data.DeviceType)  && Restrictions.Eq("Deviceindex", data.DeviceIndex) && Restrictions.IsNull("Recoveryts"))
                    //        .ListAsync<EventRecord>();
                    //    if(eventFromRecord.Count == 0)
                    //    {
                    //        EventRecord newEventRecode = new EventRecord();
                    //        newEventRecode.Createts = new DateTime(1970, 1, 1).AddSeconds(data.UnixTimestamp).ToLocalTime();
                    //        newEventRecode.Eventcode = map.Eventcode;
                    //        newEventRecode.Siteid = data.SiteId;
                    //        newEventRecode.Devicetype = data.DeviceType;
                    //        newEventRecode.Deviceindex = data.DeviceIndex;
                    //        await session.InsertAsync(newEventRecode);
                    //        //await Publisher.PublishEvent(data.SiteId, newEventRecode, EventStatus.New, cancellationToken);
                    //    }

                    //}
                    //else if(data.Status == EventStatus.Recovery)
                    //{
                    //    var eventFromRecord = await session.CreateCriteria<EventRecord>()
                    //        //.Add(Restrictions.IsNull("RecoveryDT"))
                    //        .Add(Restrictions.Eq("Eventcode", map.Eventcode) && Restrictions.Eq("Siteid", data.SiteId)
                    //        && Restrictions.Eq("Devicetype", data.DeviceType) && Restrictions.Eq("Deviceindex", data.DeviceIndex) && Restrictions.IsNull("Recoveryts"))
                    //        .ListAsync<EventRecord>();

                    //    foreach(EventRecord record in eventFromRecord)
                    //    {
                    //        record.Recoveryts = new DateTime(1970, 1, 1).AddSeconds(data.UnixTimestamp).ToLocalTime();
                    //        await database.HashSetAsync(redisKey, record.Eventrecordindex, bool.FalseString);
                    //        await session.UpdateAsync(record);
                    //    }
                    //}
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private async Task<IList<EventRecord>> FindNewRaiseEvent(int eventcode, ICriteria criteria)
        {
            return await criteria.Add(Restrictions.Eq("Eventcode", eventcode))
                                .Add(Restrictions.IsNull("Recoveryts")).ListAsync<EventRecord>();
        }
            
    }
}
