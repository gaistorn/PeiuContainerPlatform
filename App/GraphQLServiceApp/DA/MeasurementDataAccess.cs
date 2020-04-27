using GraphQL.Types;
using NHibernate.Criterion;
using PeiuPlatform.App.Model;
using PeiuPlatform.DataAccessor;
using PeiuPlatform.Models.Mysql;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public interface IMeasurementDataAccess
    {
        Task<MeasurementBySite> GetMeasurementBySiteAsync(ResolveFieldContext context, int site);
        Task<IEnumerable<MeasurementByRcc>> GetMeasurementsAsync(ResolveFieldContext context);

        Task<IEnumerable<MeasurementBySite>> GetMeasurementByAllSitesAsync(ResolveFieldContext context);
        Task<MeasurementByRcc> GetMeasurementByRccAsync(ResolveFieldContext context, int rcc);

        Task<Measurement> GetMeasurementSummary(ResolveFieldContext context);
    }
    public class MeasurementDataAccess : IMeasurementDataAccess
    {
        private readonly IDatabase database;
        private readonly IRedisDataAccessor redisDataAccessor;
        readonly MysqlDataAccessor mysqlDataAccessor;
        public MeasurementDataAccess(IRedisDataAccessor redisDataAccessor, MysqlDataAccessor mysqlDataAccessor)
        {
            this.redisDataAccessor = redisDataAccessor;
            database = redisDataAccessor.GetDatabase();
            this.mysqlDataAccessor = mysqlDataAccessor;
        }

        public async Task<Measurement> GetMeasurementSummary(ResolveFieldContext context)
        {
            var list = await GetMeasurementsAsync(context);
            if (list == null)
                return null;
            Measurement measurement = new Measurement();
            measurement.countofbms = list.Sum(x => x.countofbms);
            measurement.countofpcs = list.Sum(x => x.countofpcs);
            measurement.countofpv = list.Sum(x => x.countofpv);
            measurement.meanofsoc = list.Average(x => x.meanofsoc);
            measurement.countofevent = list.Sum(x => x.countofevent);
            measurement.sumofactivepower = list.Sum(x => x.sumofactivepower);
            measurement.sumofpvgeneration = list.Sum(x => x.sumofpvgeneration);
            measurement.countofsites = list.Sum(x => x.countofsites);
            measurement.sumofcharge = list.Sum(x => x.sumofcharge);
            measurement.sumofdischarge = list.Sum(x => x.sumofdischarge);
            return measurement;
        }

        public async Task<IEnumerable<MeasurementBySite>> GetMeasurementByAllSitesAsync(ResolveFieldContext context)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var siteKeys = context.GetAllSiteIds();
            List<MeasurementBySite> allsites = new List<MeasurementBySite>();
            foreach(int sizteId in siteKeys)
            {
                MeasurementBySite site = await GetMeasurementsBySiteIdAsync(sizteId);
                allsites.Add(site);
            }
            return allsites;
        }

        public async Task<MeasurementBySite> GetMeasurementBySiteAsync(ResolveFieldContext context, int site)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var siteKeys = context.GetAllSiteIds();
            if (siteKeys.Contains(site) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 사이트는 소유하지 않았습니다"));
                return null;
            }
            return await GetMeasurementsBySiteIdAsync(site);
        }

        public async Task<IEnumerable<MeasurementByRcc>> GetMeasurementsAsync(ResolveFieldContext context)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var siteKeys = context.GetSiteKeysByRcc();
            List<MeasurementByRcc> result = new List<MeasurementByRcc>();
            foreach(int rcc in siteKeys.Keys)
            {
                MeasurementByRcc measurementByRcc = await GetMeasurementByRccAsync(context, rcc);
                if (measurementByRcc == null)
                    continue;
                result.Add(measurementByRcc);
            }
            return result;
        }

        public async Task<MeasurementByRcc> GetMeasurementByRccAsync(ResolveFieldContext context, int rcc)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            MeasurementByRcc measurementByRcc = new MeasurementByRcc();
            List<double> socs = new List<double>();
            foreach (int siteid in rcckeys[rcc])
            {
                MeasurementBySite site = await GetMeasurementsBySiteIdAsync(siteid);
                socs.Add(site.meanofsoc);
                measurementByRcc.countofbms += site.countofbms;
                measurementByRcc.countofpcs += site.countofpcs;
                measurementByRcc.countofpv += site.countofpv;
                measurementByRcc.countofevent += site.countofevent;
                measurementByRcc.sumofactivepower += site.sumofactivepower;
                measurementByRcc.sumofpvgeneration += site.sumofpvgeneration;
                measurementByRcc.sumofcharge += site.sumofcharge;
                measurementByRcc.sumofdischarge += site.sumofdischarge;
                
            }
            measurementByRcc.meanofsoc = socs.Average();
            measurementByRcc.countofsites = rcckeys[rcc].Count;
            measurementByRcc.rcc = rcc;
            return measurementByRcc;
        }

        private async Task<MeasurementBySite> GetMeasurementsBySiteIdAsync(int SiteId)
        {
            IEnumerable<string> pcsKeys = await GetKeys(SiteId, 1);
            IEnumerable<string> bmsKeys = await GetKeys(SiteId, 2);
            IEnumerable<string> pvKeys = await GetKeys(SiteId, 4);

            var actPws = await GetValues(pcsKeys, "actPwrKw");
            var socValues = await GetValues(bmsKeys, "bms_soc");
            var pvValues = await GetValues(pvKeys, "TotalActivePower");

            int cntOfPcs = actPws.Count();
            int cntOfBms = socValues.Count();
            int cntOfPv = pvValues.Count();
            int cntOfEvent = await GetCountOfActiveEvents(new int[] { SiteId });

            MeasurementBySite measurement = new MeasurementBySite()
            {
                siteid = SiteId,
                meanofsoc = cntOfBms > 0 ? socValues.Average() : 0,
                sumofpvgeneration = cntOfPv > 0 ? pvValues.Sum() : 0,
                countofpcs = cntOfPcs,
                countofevent = cntOfEvent,
                countofbms = cntOfBms,
                countofpv = cntOfPv,
                sumofactivepower = cntOfPcs > 0 ? actPws.Sum() : 0,
                sumofcharge = actPws.Where(x=>x < 0).Sum(),
                sumofdischarge = actPws.Where(x=>x>0).Sum()


            };
            return measurement;
        }

        private async Task<int> GetCountOfActiveEvents(IEnumerable<int> sites)
        {
            try

            {
                using (var session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
                {
                    var eventss = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.IsNull("Recoveryts") && Restrictions.In("SiteId", sites.ToArray()))
                        .ListAsync<VwEventRecord>();
                    return eventss.Count;
                }
                
            }
            catch(Exception ex)
            {
                return 0;
            }
                
        }

        private async Task<IEnumerable<double>> GetValues(IEnumerable<string> keys, string fieldName)
        {
            List<double> values = new List<double>();
            foreach (string redisKey in keys)
            {
                double v = (double)await database.HashGetAsync(redisKey, fieldName);
                values.Add(v);
            }
            return values;
        }

        private async Task<IEnumerable<string>> GetKeys(int SiteId, int groupId)
        {
            int idx = 1;
            List<string> request_keys = new List<string>();
            string deviceName = "";
            switch (groupId)
            {
                case 1: deviceName = "PCS"; break;
                case 2: deviceName = "BMS"; break;
                case 4: deviceName = "PV"; break;
                default: return request_keys;
            }

            while (true)
            {
                string keyName = GetKey(SiteId, groupId, deviceName, idx++);
                if (await database.KeyExistsAsync(keyName))
                {
                    request_keys.Add(keyName);
                }
                else
                    break;
            }
            return request_keys;
        }

        private string GetKey(int SiteId, int groupId, string deviceName, int Index = 1) => $"SID{SiteId}.GID{groupId}.{deviceName}{Index}";
    }
}
