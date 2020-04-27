using GraphQL.Types;
using PeiuPlatform.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Queries
{
    
    public class DailyRevenueQueryType : ObjectGraphType<DailyRevenueQueries>
    {
        public DailyRevenueQueryType(IRevenueDailyQuery query)
        {
            Name = "DailyRevenueQueries";

            Func<ResolveFieldContext, object> GetSumRevenue = (context) => query.DailyRevenue(context);

            FieldDelegate<DailyRevenueBySummaryType>("summary", "요약", resolve: GetSumRevenue);

            Func<ResolveFieldContext, object> GetDailyRevenues = (context) => query.DailyRevenueByAll(context);
            Func<ResolveFieldContext, object> GetDailyRevenuesBySites = (context) => query.DailyRevenueByAllSites(context);
            Func<ResolveFieldContext, int, object> GetDailyRevenueByRcc = (context, rcc) => query.DailyRevenueByRcc(context, rcc);
            FieldDelegate<ListGraphType<RevenueByRccType>>(
                "allrccrevenue", resolve: GetDailyRevenues, description: "전체 RCC"
                );
            FieldDelegate<ListGraphType<RevenueBySiteType>>(
                "allsitesrevenue", resolve: GetDailyRevenuesBySites, description: "전체 Sites"
                );

            FieldDelegate<RevenueByRccType>("revenuebyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                resolve: GetDailyRevenueByRcc, description: "특정 RCC");

            //Field(f => f.revenuerccs).Description("rcc 리스트들");


            Func<ResolveFieldContext, int, object> GetDailyRevenueSiteaByRcc = (context, rcc) => query.DailyRevenueSitesByRcc(context, rcc);
            Func<ResolveFieldContext, int, object> GetDailyRevenueSiteaBySite = (context, siteid) => query.DailyRevenueSitesBysite(context, siteid);

            FieldDelegate<RevenueBySiteType>("revenuebysite",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "siteid", Description = "site 번호" }),
                resolve: GetDailyRevenueSiteaBySite, description: "특정 site");

            FieldDelegate<ListGraphType<RevenueBySiteType>>("revenuesitesbyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                description: "특정 rcc지역의 사이트들",
                resolve: GetDailyRevenueSiteaByRcc);
        }
    }
}
