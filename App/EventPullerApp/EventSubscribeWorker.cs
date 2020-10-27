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
using System.Collections.Concurrent;
using PeiuPlatform.Notification;

namespace PeiuPlatform.App
{
    public class EventSubscribeWorker : DataSubscribeWorker
    {
        readonly EventDataAccessor eventDataAccessor;
        readonly IDatabaseAsync database;
        readonly ILogger<EventSubscribeWorker> logger;
        readonly Notificator Notificator;
        public EventSubscribeWorker(ILogger<EventSubscribeWorker> _logger, IRedisDataAccessor _redis, EventDataAccessor eventDataAccessor,
            Notificator notificator           
            )
        {
            this.eventDataAccessor = eventDataAccessor;
            database = _redis.GetDatabase();
            logger = _logger;
            this.Notificator = notificator;
            
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
                               .Add(
                               Restrictions.Eq("Groupcode", data.GroupCode) &&
                               Restrictions.Eq("Factorycode", data.FactoryCode)
                               )
                               .ListAsync<EventMap>();
            return map;
        }

        private async Task<IList<EventMap>> GetEventMaps(ISession session, EventModel data, IEnumerable<UInt32> bitValues)
        {
            IList<EventMap> map = await session.CreateCriteria<EventMap>()
                               .Add(Restrictions.Eq("Groupcode", data.GroupCode) &&
                               Restrictions.Eq("Factorycode", data.FactoryCode) &&
                               Restrictions.InG<UInt32>("Bitflag", bitValues))
                               .ListAsync<EventMap>();
            return map;
        }

        private async Task<EventMap> GetEventMap(ISession session, EventModel data, UInt32 bitValue, bool CreateMapWhenNotFound = true)
        {
            EventMap map = await session.CreateCriteria<EventMap>()
                               .Add(Restrictions.Eq("Groupcode", data.GroupCode) &&
                               Restrictions.Eq("Factorycode", data.FactoryCode) &&
                               Restrictions.Eq("Bitflag", bitValue))
                               .UniqueResultAsync<EventMap>();
            if (map == null && CreateMapWhenNotFound)
            {
                logger.LogWarning($"다음과 같은 이벤트를 못찾음. FC:{data.FactoryCode}  GC:{data.GroupCode} / BF:{bitValue} / DT:{data.DeviceType}");
                //map = new EventMap();
                //map.Factorycode = data.FactoryCode;
                //map.Groupcode = data.GroupCode;
                //map.Bitflag = bitValue;
                //map.Devicetype = data.DeviceType;
                //map.Level = 0;
                //map.Name = $"Unknown(0x{bitValue.ToString("X")})";
                //await session.SaveOrUpdateAsync(map);
            }
            return map;
        }

        ConcurrentDictionary<string, UInt32> evtList = new ConcurrentDictionary<string, UInt32>();

        private void SetLatestBitValue(EventModel data, UInt32 value)
        {
            string key = data.GetUniqueKey();
            evtList[data.GetUniqueKey()] = value;
            
        }

        private UInt32 GetLatestBitValue(EventModel data)
        {
            string key = data.GetUniqueKey();
            if (evtList.TryGetValue(data.GetUniqueKey(), out UInt32 value))
                return value;
            else
                return UInt32.MaxValue;
        }

        private async Task<Notification.MessageResponse> RecoveryReportMMS(string SiteName, int SiteId, int DeviceType, int DeviceIndex, string FaultName, DateTime faultTime)
        {
            if (SiteId != 161)
                return null;
            string title = $"{SiteName} 고장복구";
            string device = "";
            switch (DeviceType)
            {
                case 0: device = "PCS"; break;
                case 1: device = "BAT"; break;
                case 2: device = "PV"; break;
            }
            string body = $"복구 시각: { faultTime.ToString("yyyy-MM-dd HH:mm:ss")}\n복구 설비: {device + DeviceIndex} \n복구 내용: {FaultName}";
            return await Notificator.SendingMMS(title, body, "01063671293", "01063671293", "01031819954", "01085752497");
        }

        private async Task<Notification.MessageResponse> FaultReportMMS(string SiteName, int SiteId, int DeviceType, int DeviceIndex, string FaultName, DateTime faultTime)
        {
            if (SiteId != 161)
                return null;
            string title = $"{SiteName} 고장발생";
            string device = "";
            switch(DeviceType)
            {
                case 0: device = "PCS"; break;
                case 1: device = "BAT"; break;
                case 2: device = "PV"; break;
            }
            string body = $"발생 시각: { faultTime.ToString("yyyy-MM-dd HH:mm:ss")}\n고장 설비: {device + DeviceIndex} \n고장 내용: {FaultName}";
            return await Notificator.SendingMMS(title, body, "01063671293", "01063671293", "01031819954", "01085752497");
            //return await Notificator.SendingMMS(title, body, "01063671293", "01063671293");
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
                
                DateTime dt = DateTime.FromFileTimeUtc(data.UnixTimestamp);
                using (var session = eventDataAccessor.SessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {

                    UInt32 latestBitValue = GetLatestBitValue(data);
                    if(latestBitValue == data.BitFlag)
                    {
                        return;
                    }
                    else
                    {
                        SetLatestBitValue(data, data.BitFlag);
                    }

                    var siteInfo = await GetSiteView(session, data.SiteId);

                    if (data.BitFlag == 0)
                    {
                        IList<EventMap> evtList = await GetEventMaps(session, data);
                        if(evtList.Count > 0)
                        {
                            var existEvents = await session.CreateCriteria<EventRecord>().Add(
                               Restrictions.InG<int>("Eventcode", evtList.Select(x=>x.Eventcode)) && 
                               Restrictions.IsNull("Recoveryts") && 
                               Restrictions.Eq("Devicetype", data.DeviceType) &&
                               Restrictions.Eq("Deviceindex", data.DeviceIndex) &&
                               Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
                            bool HasChanged = false;
                            foreach (EventRecord record in existEvents)
                            {

                                
                                record.Recoveryts = dt;
                                logger.LogWarning($"[RECOVERY EVENT] SITEID: {data.SiteId} / GC:{data.GroupCode} / BF:{data.BitFlag} / DT:{data.DeviceType}");
                                HasChanged = true;
                                await session.UpdateAsync(record);
                                EventMap map = evtList.FirstOrDefault(x => x.Eventcode == record.Eventcode);
                                await RecoveryReportMMS(siteInfo.Represenation, data.SiteId, data.DeviceType, data.DeviceIndex, map.Name, dt);

                            }
                            if (HasChanged)
                            {
                                await transaction.CommitAsync();
                            }
                            return;
                        }
                    }

                    List<UInt32> FAULT_BITS = new List<UInt32>();
                    List<UInt32> NORMAL_BITS = new List<UInt32>();
                    for (UInt32 i = 0; i < 32; i++)
                    {
                        UInt32 bit = (UInt32)Math.Pow(2, i);
                        if ((data.BitFlag & bit) == bit)
                            FAULT_BITS.Add(bit);
                        else
                            NORMAL_BITS.Add(bit);
                    }

                    await FaultProcessing(session, data, siteInfo, FAULT_BITS);
                    await RecoveryProcessing(session, data, siteInfo, NORMAL_BITS);
                    await transaction.CommitAsync();                   
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private async Task<Vwcontractorsite> GetSiteView(ISession session, int SiteId)
        {
            return await session.CreateCriteria<Vwcontractorsite>().Add(
                                   Restrictions.Eq("Siteid", SiteId)).UniqueResultAsync<Vwcontractorsite>();
        }

        private async Task<VwEventRecord> GetEventRecordView(ISession session, int Id)
        {
            return  await session.CreateCriteria<VwEventRecord>().Add(
                                   Restrictions.Eq("Eventcode", Id)).UniqueResultAsync<VwEventRecord>();
        }

        private async Task RecoveryProcessing(ISession session, EventModel data, Vwcontractorsite siteInfo, IEnumerable<UInt32> bitflags)
        {
            IList<EventMap> map = await GetEventMaps(session, data, bitflags);
            if (map == null || map.Count == 0)    // 없는 이벤트 일 경우 무시
                return;
            var existEvents = await session.CreateCriteria<EventRecord>().Add(
                Restrictions.InG<int>("Eventcode", map.Select(x=>x.Eventcode)) && Restrictions.IsNull("Recoveryts") &&
                Restrictions.Eq("Devicetype", data.DeviceType) &&
               Restrictions.Eq("Deviceindex", data.DeviceIndex) &&
                Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
            foreach (EventRecord record in existEvents)
            {
                DateTime dt = DateTime.FromFileTime(data.UnixTimestamp);
                record.Recoveryts = dt;
                await session.UpdateAsync(record);
                EventMap target_map = map.FirstOrDefault(x => x.Eventcode == record.Eventcode);
                await RecoveryReportMMS(siteInfo.Represenation, data.SiteId, data.DeviceType, data.DeviceIndex, target_map.Name, dt);

                logger.LogWarning($"[RECOVERY EVENT] SITEID: {data.SiteId} / GC:{data.GroupCode} / BF:tail{data.BitFlag} / DT:{data.DeviceType} / INDEX:{data.DeviceIndex}");
            }
        }

        private async Task FaultProcessing(ISession session, EventModel data, Vwcontractorsite siteInfo, IEnumerable<UInt32> bitflags)
        {
            foreach (UInt32 bit in bitflags)
            {
                // 발생한 이벤트가 이미 존재하는지 체크
                EventMap map = await GetEventMap(session, data, bit);
                // 복구 이벤트 체크
                if (map == null)
                    continue;
                var existEvents = await session.CreateCriteria<EventRecord>().Add(
                    Restrictions.Eq("Eventcode", map.Eventcode) && Restrictions.IsNull("Recoveryts") &&
                    Restrictions.Eq("Devicetype", data.DeviceType) &&
                   Restrictions.Eq("Deviceindex", data.DeviceIndex) &&
                    Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
                // 이벤트가 발생했는데, 기존에 엑티브 이벤트에 없을 경우 이벤트를 인서트 한다.
                if (existEvents.Count == 0)
                {
                    EventRecord newEventRecode = new EventRecord();
                    DateTime dt = DateTime.FromFileTime(data.UnixTimestamp);
                    newEventRecode.Createts = dt;
                    newEventRecode.Eventcode = map.Eventcode;
                    newEventRecode.Siteid = data.SiteId;
                    newEventRecode.Devicetype = data.DeviceType;
                    newEventRecode.Deviceindex = data.DeviceIndex;
                    await session.SaveOrUpdateAsync(newEventRecode);
                    await FaultReportMMS(siteInfo.Represenation, data.SiteId, data.DeviceType, data.DeviceIndex, map.Name, dt);
                    logger.LogWarning($"[NEW EVENT] SITEID: {data.SiteId} / GC:{data.GroupCode} / BF:{bit} / DT:{data.DeviceType} / INDEX:{data.DeviceIndex}");
                }
            }
        }

        //private async Task FaultRecover(ISession session, EventModel data)
        //{
        //    if (data.BitFlag == 0)
        //    {
        //        IList<EventMap> evtList = await GetEventMaps(session, data);
        //        if (evtList.Count > 0)
        //        {
        //            var existEvents = await session.CreateCriteria<EventRecord>().Add(
        //               Restrictions.InG<int>("Eventcode", evtList.Select(x => x.Eventcode)) &&
        //               Restrictions.IsNull("Recoveryts") &&
        //               Restrictions.Eq("Devicetype", data.DeviceType) &&
        //               Restrictions.Eq("Deviceindex", data.DeviceIndex) &&
        //               Restrictions.Eq("Siteid", data.SiteId)).ListAsync<EventRecord>();
        //            bool HasChanged = false;
        //            foreach (EventRecord record in existEvents)
        //            {
        //                record.Recoveryts = dt;
        //                logger.LogWarning($"[RECOVERY EVENT] SITEID: {data.SiteId} / GC:{data.GroupCode} / BF:{data.BitFlag} / DT:{data.DeviceType}");
        //                HasChanged = true;
        //                await session.UpdateAsync(record);
        //            }
        //            if (HasChanged)
        //            {
        //                await transaction.CommitAsync();
        //            }
        //            return;
        //        }
        //    }
        //}

        private async Task<IList<EventRecord>> FindNewRaiseEvent(int eventcode, ICriteria criteria)
        {
            return await criteria.Add(Restrictions.Eq("Eventcode", eventcode))
                                .Add(Restrictions.IsNull("Recoveryts")).ListAsync<EventRecord>();
        }
            
    }
}
