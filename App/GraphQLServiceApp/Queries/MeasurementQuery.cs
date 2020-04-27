using GraphQL.Types;
using PeiuPlatform.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Queries
{
    public class MeasurementQuery
    { }
    public class MeasurementQueryType : ObjectGraphType<MeasurementQuery>
    {
        public MeasurementQueryType(IMeasurementDataAccess query)
        {
            Name = "MeasurementQueries";

            Func<ResolveFieldContext, int, object> GetMeasurementByRccFunc = (context, rcc) => query.GetMeasurementByRccAsync(context, rcc);
            Func<ResolveFieldContext, int, object> GetMeasurementBySite = (context, siteid) => query.GetMeasurementBySiteAsync(context, siteid);
            Func<ResolveFieldContext, object> GetMeasurementByAllRccFunc = (context) => query.GetMeasurementsAsync(context);
            Func<ResolveFieldContext, object> GetMeasurementByAllSiteFunc = (context) => query.GetMeasurementByAllSitesAsync(context);
            Func<ResolveFieldContext, object> GetMeasurementSummaryFunc = (context) => query.GetMeasurementSummary(context);



            FieldDelegate<ListGraphType<MeasurementByRccType>>(
                "allmeasurementrcc", resolve: GetMeasurementByAllRccFunc, description: "전체 RCC"
                );

            FieldDelegate<ListGraphType<MeasurementBySiteType>>(
               "allmeasurementsites", resolve: GetMeasurementByAllSiteFunc, description: "전체 Sites"
               );

            FieldDelegate<MeasurementType>("measurementsummary", resolve: GetMeasurementSummaryFunc, description: "전체 요약");

            FieldDelegate<MeasurementByRccType>("measurementbyrcc",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "rcc", Description = "rcc 번호" }),
                resolve: GetMeasurementByRccFunc, description: "특정 RCC");

            FieldDelegate<MeasurementBySiteType>("measurementbysite",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "siteid", Description = "site 번호" }),
                resolve: GetMeasurementBySite, description: "특정 site");
        }
    }
}
