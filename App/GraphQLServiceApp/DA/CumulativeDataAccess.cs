using GraphQL.Types;
using NHibernate;
using NHibernate.Criterion;
using PeiuPlatform.App.Model;
using PeiuPlatform.DataAccessor;
using PeiuPlatform.Models;
using PeiuPlatform.Models.Mysql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public interface ICumulativeDailyQuery
    {
        Task<IEnumerable<CumulativeByRcc>> DailyCumulateByAll(ResolveFieldContext resolveFieldContext);
        Task<IEnumerable<CumulativeBySite>> DailyCumulateSitesByRcc(ResolveFieldContext context, int rcc);
        Task<CumulativeBySite> DailyCumulateSitesBysite(ResolveFieldContext context, int site);
        Task<CumulativeByRcc> DailyCumulateByRcc(ResolveFieldContext resolveFieldContext, int rcc);
        Task<AnalogSummary> DailyCumulateSummary(ResolveFieldContext context);
    }

    public interface ICumulativeMonthlyQuery
    {
        Task<IEnumerable<CumulativeByRcc>> MonthlyCumulateByAll(ResolveFieldContext resolveFieldContext);
        Task<IEnumerable<CumulativeBySite>> MonthlyCumulateSitesByRcc(ResolveFieldContext context, int rcc);
        Task<CumulativeBySite> MonthlyCumulateSitesBysite(ResolveFieldContext context, int site);
        Task<CumulativeByRcc> MonthlyCumulateByRcc(ResolveFieldContext resolveFieldContext, int rcc);
        Task<AnalogSummary> MonthlyCumulateSummary(ResolveFieldContext context);
    }

    public class CumulativeDataAccess : ICumulativeDailyQuery, ICumulativeMonthlyQuery
    {
        readonly MysqlDataAccessor mysqlDataAccessor;
        public CumulativeDataAccess(MysqlDataAccessor mysqlDataAccessor)
        {
            this.mysqlDataAccessor = mysqlDataAccessor;
        }

        #region Common Query

        private async Task<AnalogSummary> GetMonthlySummaryAsync(ResolveFieldContext context, int year, int month)
        {
            try
            {
                if (context.ThrowIfInvalidAuthorization() == false)
                    return null;

                var sites = context.GetAllSiteIds();
                AnalogSummary summary = new AnalogSummary();

                using (var session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
                {
                    var result = await MonthlyQuery(session, year, month, sites);
                    double Sumofcharge = result.Sum(x => x.Sumofcharge);
                    double Sumofdischarge = result.Sum(x => x.Sumofdischarge);
                    double sumOfPv = result.Sum(x => x.Sumofpvgeneration);

                    summary.sumofcharge = Sumofcharge;
                    summary.sumofdischarge = Sumofdischarge;
                    summary.sumofpvgeneration = sumOfPv;
                    return summary;
                }
            }
            catch (Exception ex)
            {
                context.Errors.Add(new GraphQL.ExecutionError(ex.Message, ex));
                return null;
            }
        }

        private async Task<AnalogSummary> GetDailySummaryAsync(ResolveFieldContext context, DateTime start)
        {
            try
            {
                if (context.ThrowIfInvalidAuthorization() == false)
                    return null;

                var sites = context.GetAllSiteIds();
                AnalogSummary summary = new AnalogSummary();

                using(var session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
                {
                    var result = await session.CreateCriteria<HourlyAccmofMeasurement>()
                        .Add(Restrictions.Eq("Createdt", start.Date) && Restrictions.In("Siteid", sites.ToArray())).ListAsync<HourlyAccmofMeasurement>();
                    double Sumofcharge = result.Sum(x => x.Sumofcharge);
                    double Sumofdischarge = result.Sum(x => x.Sumofdischarge);
                    double sumOfPv = result.Sum(x => x.Sumofpvgeneration);

                    summary.sumofcharge = Sumofcharge;
                    summary.sumofdischarge = Sumofdischarge;
                    summary.sumofpvgeneration = sumOfPv;
                    return summary;
                }
            }
            catch (Exception ex)
            {
                context.Errors.Add(new GraphQL.ExecutionError(ex.Message, ex));
                return null;
            }
        }

        private async Task<IList<HourlyAccmofMeasurement>> DailyQuery(IStatelessSession session, DateTime date, IEnumerable<int> Siteids)
        {
            var result = await session.CreateCriteria<HourlyAccmofMeasurement>()
                    .Add(Restrictions.Eq("Createdt", date.Date) && Restrictions.In("Siteid", Siteids.ToArray())).ListAsync<HourlyAccmofMeasurement>();
            return result;
        }


        private async Task<IList<DailyAccmofMeasurement>> MonthlyQuery(IStatelessSession session, int year, int month, IEnumerable<int> Siteids)
        {
            var result = await session.CreateCriteria<DailyAccmofMeasurement>()
                .Add(Restrictions.Where<DailyAccmofMeasurement>(x=>x.Createdt.Year == year && x.Createdt.Month == month))
                .Add(Restrictions.In("Siteid", Siteids.ToArray())).ListAsync<DailyAccmofMeasurement>();
            return result;
        }

        #endregion

        #region Daily query

        public async Task<AnalogSummary> DailyCumulateSummary(ResolveFieldContext context)
        {
            return await GetDailySummaryAsync(context, DateTime.Now.Date);
        }

        public async Task<IEnumerable<CumulativeByRcc>> DailyCumulateByAll(ResolveFieldContext context)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            List<CumulativeByRcc> results = new List<CumulativeByRcc>();
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                foreach (int rcc in rcckeys.Keys)
                {
                    var result = await DailyQuery(session, DateTime.Now.Date, rcckeys[rcc]);
                    CumulativeByRcc row = new CumulativeByRcc();
                    row.rcc = rcc;
                    row.sumofcharge = result.Sum(x => x.Sumofcharge);
                    row.sumofdischarge = result.Sum(x => x.Sumofdischarge);
                    row.sumofpvgeneration = result.Sum(x => x.Sumofpvgeneration);
                    row.sites = result.Select(x => new CumulativeBySite() { siteid = x.Siteid, sumofcharge = x.Sumofcharge, sumofdischarge = x.Sumofdischarge, sumofpvgeneration = x.Sumofpvgeneration });
                    results.Add(row);
                }
            }
            return results;
        }

        public async Task<CumulativeByRcc> DailyCumulateByRcc(ResolveFieldContext context, int rcc)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await DailyQuery(session, DateTime.Now.Date, rcckeys[rcc]);
                CumulativeByRcc row = new CumulativeByRcc();
                row.rcc = rcc;
                row.sumofcharge = result.Sum(x => x.Sumofcharge);
                row.sumofdischarge = result.Sum(x => x.Sumofdischarge);
                row.sumofpvgeneration = result.Sum(x => x.Sumofpvgeneration);
                row.sites = result.Select(x => new CumulativeBySite() { siteid = x.Siteid, sumofcharge = x.Sumofcharge, sumofdischarge = x.Sumofdischarge, sumofpvgeneration = x.Sumofpvgeneration });
                return row;
            }
        }

        public async Task<CumulativeBySite> DailyCumulateSitesBysite(ResolveFieldContext context, int site)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var Siteids = context.GetAllSiteIds();
            if (Siteids.Contains(site) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await DailyQuery(session, DateTime.Now.Date, new int[] { site });
                if(result.Count > 0)
                {
                    CumulativeBySite row = new CumulativeBySite();
                    row.siteid = site;
                    row.sumofcharge = result.Sum(x => x.Sumofcharge);
                    row.sumofdischarge = result.Sum(x => x.Sumofdischarge);
                    row.sumofpvgeneration = result.Sum(x => x.Sumofpvgeneration);
                    return row;
                }
                else
                {
                    context.Errors.Add(new GraphQL.ExecutionError("대상 지역 사이트는 소유하지 않았습니다"));
                    return null;
                }
            }
        }

        public async Task<IEnumerable<CumulativeBySite>> DailyCumulateSitesByRcc(ResolveFieldContext context, int rcc)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await DailyQuery(session, DateTime.Now.Date, rcckeys[rcc]);
                return result.Select(x => new CumulativeBySite() { siteid = x.Siteid, sumofcharge = x.Sumofcharge, sumofdischarge = x.Sumofdischarge, sumofpvgeneration = x.Sumofpvgeneration });
            }
        }

        #endregion

        #region Monthly queries
        public async Task<AnalogSummary> MonthlyCumulateSummary(ResolveFieldContext context)
        {
            return await GetMonthlySummaryAsync(context, DateTime.Now.Year, DateTime.Now.Month);
        }

        public async Task<IEnumerable<CumulativeByRcc>> MonthlyCumulateByAll(ResolveFieldContext context)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            List<CumulativeByRcc> results = new List<CumulativeByRcc>();
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                foreach (int rcc in rcckeys.Keys)
                {
                    var result = await MonthlyQuery(session, DateTime.Now.Year, DateTime.Now.Month, rcckeys[rcc]);
                    CumulativeByRcc row = new CumulativeByRcc();
                    row.rcc = rcc;
                    row.sumofcharge = result.Sum(x => x.Sumofcharge);
                    row.sumofdischarge = result.Sum(x => x.Sumofdischarge);
                    row.sumofpvgeneration = result.Sum(x => x.Sumofpvgeneration);
                    row.sites = result.Select(x => new CumulativeBySite() { siteid = x.Siteid, sumofcharge = x.Sumofcharge, sumofdischarge = x.Sumofdischarge, sumofpvgeneration = x.Sumofpvgeneration });
                    results.Add(row);
                }
            }
            return results;
        }

        public async Task<CumulativeByRcc> MonthlyCumulateByRcc(ResolveFieldContext context, int rcc)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await MonthlyQuery(session, DateTime.Now.Year, DateTime.Now.Month, rcckeys[rcc]);
                CumulativeByRcc row = new CumulativeByRcc();
                row.rcc = rcc;
                row.sumofcharge = result.Sum(x => x.Sumofcharge);
                row.sumofdischarge = result.Sum(x => x.Sumofdischarge);
                row.sumofpvgeneration = result.Sum(x => x.Sumofpvgeneration);
                row.sites = result.Select(x => new CumulativeBySite() { siteid = x.Siteid, sumofcharge = x.Sumofcharge, sumofdischarge = x.Sumofdischarge, sumofpvgeneration = x.Sumofpvgeneration });
                return row;
            }
        }

        public async Task<CumulativeBySite> MonthlyCumulateSitesBysite(ResolveFieldContext context, int site)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var Siteids = context.GetAllSiteIds();
            if (Siteids.Contains(site) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await MonthlyQuery(session, DateTime.Now.Year, DateTime.Now.Month, new int[] { site });
                if (result.Count > 0)
                {
                    CumulativeBySite row = new CumulativeBySite();
                    row.siteid = site;
                    row.sumofcharge = result.Sum(x => x.Sumofcharge);
                    row.sumofdischarge = result.Sum(x => x.Sumofdischarge);
                    row.sumofpvgeneration = result.Sum(x => x.Sumofpvgeneration);
                    return row;
                }
                else
                {
                    context.Errors.Add(new GraphQL.ExecutionError("대상 지역 사이트는 소유하지 않았습니다"));
                    return null;
                }
            }
        }

        public async Task<IEnumerable<CumulativeBySite>> MonthlyCumulateSitesByRcc(ResolveFieldContext context, int rcc)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await MonthlyQuery(session, DateTime.Now.Year, DateTime.Now.Day, rcckeys[rcc]);
                return result.Select(x => new CumulativeBySite() { siteid = x.Siteid, sumofcharge = x.Sumofcharge, sumofdischarge = x.Sumofdischarge, sumofpvgeneration = x.Sumofpvgeneration });
            }
        }

        #endregion



    }
}
