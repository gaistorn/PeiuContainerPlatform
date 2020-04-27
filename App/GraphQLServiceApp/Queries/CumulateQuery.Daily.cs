using GraphQL.Types;
using PeiuPlatform.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Queries
{
    
    public class DailyCumulateQueryType : ObjectGraphType<DailyCumulateQueries>
    {
        public DailyCumulateQueryType(ICumulativeDailyQuery query)
        {
            Name = "DailyCumulateQueries";

            Func<ResolveFieldContext, object> GetSumCumulate = (context) => query.DailyCumulateSummary(context);

            FieldDelegate<AnalogSummaryType>("summary", "요약", resolve: GetSumCumulate);

            Func<ResolveFieldContext, object> GetDailyCumulate = (context) => query.DailyCumulateByAll(context);
            Func<ResolveFieldContext, int, object> GetDailyCumulateByRcc = (context, rcc) => query.DailyCumulateByRcc(context, rcc);

            FieldDelegate<ListGraphType<CumulativeByRccType>>(
                "allrccrevenue", resolve: GetDailyCumulate, description: "전체 RCC"
                );


            FieldDelegate<CumulativeByRccType>("cumulatebyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                resolve: GetDailyCumulateByRcc, description: "특정 RCC");

            //Field(f => f.revenuerccs).Description("rcc 리스트들");


            Func<ResolveFieldContext, int, object> GetDailyCumulateSiteByRcc = (context, rcc) => query.DailyCumulateSitesByRcc(context, rcc);
            Func<ResolveFieldContext, int, object> GetDailyCumulateSiteBySite = (context, siteid) => query.DailyCumulateSitesBysite(context, siteid);

            FieldDelegate<CumulativeBySiteType>("cumulatebysite",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "siteid", Description = "site 번호" }),
                resolve: GetDailyCumulateSiteBySite, description: "특정 site");

            FieldDelegate<ListGraphType<CumulativeBySiteType>>("cumulatesitesbyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                description: "특정 rcc지역의 사이트들",
                resolve: GetDailyCumulateSiteByRcc);
        }
    }
}
