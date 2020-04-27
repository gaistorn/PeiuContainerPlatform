using GraphQL.Types;
using PeiuPlatform.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Queries
{
    
    public class MonthlyRevenueQueryType : ObjectGraphType<MonthlyRevenueQueries>
    {
        public MonthlyRevenueQueryType(IRevenueMonthlyQuery query)
        {
            Name = "monthlyqueries";

            Func<ResolveFieldContext, object> GetSumRevenue = (context) => query.MonthlyRevenue(context);

            FieldDelegate<MonthlyRevenueBySummaryType>("summary", "요약", resolve: GetSumRevenue);

            Func<ResolveFieldContext, object> GetMonthlyRevenues = (context) => query.MonthlyRevenueByAll(context);
            Func<ResolveFieldContext, int, object> GetMonthlyRevenueByRcc = (context, rcc) => query.MonthlyRevenueByRcc(context, rcc);
            FieldDelegate<ListGraphType<RevenueByRccType>>(
                "allrccrevenue", resolve: GetMonthlyRevenues, description: "전체 RCC"
                );

            FieldDelegate<RevenueByRccType>("revenuebyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                resolve: GetMonthlyRevenueByRcc, description: "특정 RCC");

            //Field(f => f.revenuerccs).Description("rcc 리스트들");


            Func<ResolveFieldContext, int, object> GetMonthlyRevenueSiteaByRcc = (context, rcc) => query.MonthlyRevenueSitesByRcc(context, rcc);
            Func<ResolveFieldContext, int, object> GetMonthlyRevenueSiteaBySite = (context, siteid) => query.MonthlyRevenueSitesBysite(context, siteid);

            FieldDelegate<RevenueBySiteType>("revenuebysite",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "siteid", Description = "site 번호" }),
                resolve: GetMonthlyRevenueSiteaBySite, description: "특정 site");

            FieldDelegate<ListGraphType<RevenueBySiteType>>("revenuesitesbyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                description: "특정 rcc지역의 사이트들",
                resolve: GetMonthlyRevenueSiteaByRcc);
        }
    }
}
