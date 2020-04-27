using GraphQL.Types;
using PeiuPlatform.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Queries
{
    
    public class MonthlyCumulateQueryType : ObjectGraphType<MonthlyCumulateQueries>
    {
        public MonthlyCumulateQueryType(ICumulativeMonthlyQuery query)
        {
            Name = "MonthlyCumulateQueries";

            Func<ResolveFieldContext, object> GetSumCumulate = (context) => query.MonthlyCumulateSummary(context);

            FieldDelegate<AnalogSummaryType>("summary", "요약", resolve: GetSumCumulate);

            Func<ResolveFieldContext, object> GetMonthlyCumulate = (context) => query.MonthlyCumulateByAll(context);
            Func<ResolveFieldContext, int, object> GetMonthlyCumulateByRcc = (context, rcc) => query.MonthlyCumulateByRcc(context, rcc);
            FieldDelegate<ListGraphType<CumulativeByRccType>>(
                "allrccrevenue", resolve: GetMonthlyCumulate, description: "전체 RCC"
                );

            FieldDelegate<CumulativeByRccType>("cumulatebyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                resolve: GetMonthlyCumulateByRcc, description: "특정 RCC");

            //Field(f => f.revenuerccs).Description("rcc 리스트들");


            Func<ResolveFieldContext, int, object> GetMonthlyCumulateSiteByRcc = (context, rcc) => query.MonthlyCumulateSitesByRcc(context, rcc);
            Func<ResolveFieldContext, int, object> GetMonthlyCumulateSiteBySite = (context, siteid) => query.MonthlyCumulateSitesBysite(context, siteid);

            FieldDelegate<CumulativeBySiteType>("cumulatebysite",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "siteid", Description = "site 번호" }),
                resolve: GetMonthlyCumulateSiteBySite, description: "특정 site");

            FieldDelegate<ListGraphType<CumulativeBySiteType>>("cumulatesitesbyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                description: "특정 rcc지역의 사이트들",
                resolve: GetMonthlyCumulateSiteByRcc);
        }
    }
}
