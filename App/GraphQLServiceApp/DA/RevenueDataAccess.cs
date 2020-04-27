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
    public interface IRevenueDailyQuery
    {
        Task<IEnumerable<RevenueModelByRcc>> DailyRevenueByAll(ResolveFieldContext resolveFieldContext);
        Task<IEnumerable<RevenueBySite>> DailyRevenueSitesByRcc(ResolveFieldContext context, int rcc);
        Task<RevenueBySite> DailyRevenueSitesBysite(ResolveFieldContext context, int site);
        Task<RevenueModelByRcc> DailyRevenueByRcc(ResolveFieldContext resolveFieldContext, int rcc);
        Task<RevenueBySummary> DailyRevenue(ResolveFieldContext context);

        Task<IEnumerable<RevenueBySite>> DailyRevenueByAllSites(ResolveFieldContext context);
    }

    public interface IRevenueMonthlyQuery
    {
        Task<IEnumerable<RevenueModelByRcc>> MonthlyRevenueByAll(ResolveFieldContext resolveFieldContext);
        Task<IEnumerable<RevenueBySite>> MonthlyRevenueSitesByRcc(ResolveFieldContext context, int rcc);
        Task<RevenueBySite> MonthlyRevenueSitesBysite(ResolveFieldContext context, int site);
        Task<RevenueModelByRcc> MonthlyRevenueByRcc(ResolveFieldContext resolveFieldContext, int rcc);
        Task<RevenueBySummary> MonthlyRevenue(ResolveFieldContext context);
    }

    public class RevenueDataAccess : IRevenueDailyQuery, IRevenueMonthlyQuery
    {
        readonly MysqlDataAccessor mysqlDataAccessor;
        public RevenueDataAccess(MysqlDataAccessor mysqlDataAccessor)
        {
            this.mysqlDataAccessor = mysqlDataAccessor;
        }
        delegate Task<IList<T>> QueryTemplate<T>(IStatelessSession session, DateTime date, IEnumerable<int> siteIds) where T : IRevenue;

        #region Common Query

        private async Task<IList<HourlyActualRevenue>> DailyQuery(IStatelessSession session, DateTime date, IEnumerable<int> siteIds)
        {
            var result = await session.CreateCriteria<HourlyActualRevenue>()
                    .Add(Restrictions.Eq("Createdt", date.Date) && Restrictions.In("Siteid", siteIds.ToArray()))
                    .AddOrder(new Order("Revenue", false))
                    .ListAsync<HourlyActualRevenue>();
            return result;
        }

        private async Task<IList<DailyActualRevenue>> MonthlyQuery(IStatelessSession session, DateTime date, IEnumerable<int> siteIds)
        {
            var result = await session.CreateCriteria<DailyActualRevenue>()
                .Add(Restrictions.Where<DailyActualRevenue>(x=>x.Createdt.Year == date.Year && x.Createdt.Month == date.Month))
                .Add(Restrictions.In("Siteid", siteIds.ToArray())).ListAsync<DailyActualRevenue>();
            return result;
        }

        private async Task<RevenueBySummary> GetRevenueBySummaryAsync<RType>(ResolveFieldContext context, QueryTemplate<RType> query, DateTime date) where RType : IRevenue
        {
            try
            {
                
                if (context.ThrowIfInvalidAuthorization() == false)
                    return null;

                var keys = context.GetAllSiteIds();
                using(IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
                {
                    var result = await query(session, date, keys);
                    RevenueBySummary summary = new RevenueBySummary();
                    double money = result.Sum(x => x.Revenue);
                    summary.sumofmoney = money;
                    return summary;
                }
                
            }
            catch (Exception ex)
            {
                context.Errors.Add(new GraphQL.ExecutionError(ex.Message, ex));
                return null;
            }
        }

        private async Task<IEnumerable<RevenueBySite>> RevenueByAllSites<RType>(ResolveFieldContext context, QueryTemplate<RType> query, DateTime date) where RType : IRevenue
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var sitekeys = context.GetAllSiteIds();
            List<RevenueBySite> results = new List<RevenueBySite>();
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                foreach (int site in sitekeys)
                {
                    var result = await query(session, date, new int[] { site });
                    RevenueBySite line = new RevenueBySite { siteid = site, sumofmoney = result.Sum(x => x.Revenue) };
                    results.Add(line);
                }
            }
            return results.OrderByDescending(x => x.sumofmoney);
        }

        private async Task<IEnumerable<RevenueModelByRcc>> RevenueByAll<RType>(ResolveFieldContext context, QueryTemplate<RType> query, DateTime date) where RType : IRevenue
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            List<RevenueModelByRcc> results = new List<RevenueModelByRcc>();
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                foreach (int rcc in rcckeys.Keys)
                {
                    IEnumerable<int> siteKeys = rcckeys[rcc];
                    var rows= await query(session, date, siteKeys);
                    RevenueModelByRcc line = new RevenueModelByRcc { rcc = rcc, sumofmoney = rows.Sum(x=>x.Revenue) };
                    line.sites = rows.Select(x => new RevenueBySite { siteid = x.Siteid, sumofmoney = x.Revenue });
                    results.Add(line);

                }
                return results;
            }
        }

        private async Task<RevenueModelByRcc> RevenueByRcc<RType>(ResolveFieldContext context, QueryTemplate<RType> query, DateTime date, int rcc) where RType : IRevenue
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            List<RevenueModelByRcc> results = new List<RevenueModelByRcc>();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }
            IEnumerable<int> siteKeys = rcckeys[rcc];
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await query(session, date, siteKeys);
                RevenueModelByRcc line = new RevenueModelByRcc { rcc = rcc, sumofmoney = result.Sum(x => x.Revenue) };
                return line;
            }
        }

        private async Task<RevenueBySite> RevenueSitesBysite<RType>(ResolveFieldContext context, QueryTemplate<RType> query, DateTime date, int site) where RType : IRevenue
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var siteKeys = context.GetAllSiteIds();
            if (siteKeys.Contains(site) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 사이트는 소유하지 않았습니다"));
                return null;
            }
            using (IStatelessSession session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await query(session, date, new int[] { site });
                RevenueBySite line = new RevenueBySite { siteid = site, sumofmoney = result.Sum(x => x.Revenue) };
                return line;
            }
        }

        private async Task<IEnumerable<RevenueBySite>> RevenueSitesByRcc<RType>(ResolveFieldContext context, QueryTemplate<RType> query, DateTime date, int rcc) where RType : IRevenue
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;
            var rcckeys = context.GetSiteKeysByRcc();
            List<RevenueBySite> results = new List<RevenueBySite>();
            if (rcckeys.ContainsKey(rcc) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 RCC의 지역 사이트는 소유하지 않았습니다"));
                return null;
            }

            using(var session = mysqlDataAccessor.SessionFactory.OpenStatelessSession())
            {
                var result = await query(session, date, rcckeys[rcc]);
                return result.Select(x => new RevenueBySite { siteid = x.Siteid, sumofmoney = x.Revenue });
            }
           
        }

        #endregion

        #region Daily query

        public async Task<RevenueBySummary> DailyRevenue(ResolveFieldContext context)
        {
            return await GetRevenueBySummaryAsync<HourlyActualRevenue>(context, DailyQuery, DateTime.Now);
        }

        public async Task<IEnumerable<RevenueModelByRcc>> DailyRevenueByAll(ResolveFieldContext context)
        {
            return await RevenueByAll<HourlyActualRevenue>(context, DailyQuery, DateTime.Now);
        }

        public async Task<IEnumerable<RevenueBySite>> DailyRevenueByAllSites(ResolveFieldContext context)
        {
            return await RevenueByAllSites<HourlyActualRevenue>(context, DailyQuery, DateTime.Now);
        }

        public async Task<RevenueModelByRcc> DailyRevenueByRcc(ResolveFieldContext context, int rcc)
        {
            return await RevenueByRcc<HourlyActualRevenue>(context, DailyQuery, DateTime.Now, rcc);
        }

        public async Task<RevenueBySite> DailyRevenueSitesBysite(ResolveFieldContext context, int site)
        {
            return await RevenueSitesBysite<HourlyActualRevenue>(context, DailyQuery,DateTime.Now.Date, site);
        }

        public async Task<IEnumerable<RevenueBySite>> DailyRevenueSitesByRcc(ResolveFieldContext context, int rcc)
        {
            return await RevenueSitesByRcc<HourlyActualRevenue>(context, DailyQuery, DateTime.Now, rcc);
        }

        #endregion

        #region Monthly queries
        public async Task<IEnumerable<RevenueModelByRcc>> MonthlyRevenueByAll(ResolveFieldContext context)
        {
            return await RevenueByAll<DailyActualRevenue>(context, MonthlyQuery, DateTime.Now);
        }

        public async Task<IEnumerable<RevenueBySite>> MonthlyRevenueSitesByRcc(ResolveFieldContext context, int rcc)
        {
            return await RevenueSitesByRcc<DailyActualRevenue>(context, MonthlyQuery, DateTime.Now, rcc);
        }

        public async Task<RevenueBySite> MonthlyRevenueSitesBysite(ResolveFieldContext context, int site)
        {
            return await RevenueSitesBysite<DailyActualRevenue>(context, MonthlyQuery, DateTime.Now.Date, site);
        }

        public async Task<RevenueModelByRcc> MonthlyRevenueByRcc(ResolveFieldContext context, int rcc)
        {
            return await RevenueByRcc<DailyActualRevenue>(context, MonthlyQuery, DateTime.Now, rcc);
        }

        public async Task<RevenueBySummary> MonthlyRevenue(ResolveFieldContext context)
        {
            return await GetRevenueBySummaryAsync<DailyActualRevenue>(context, MonthlyQuery, DateTime.Now);
        }

        #endregion



    }
}
