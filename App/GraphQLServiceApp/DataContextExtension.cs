using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using PeiuPlatform.App.Model;
using PeiuPlatform.App.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public static class DataContextExtension
    {
        //const string ENV_INFLUXDB_HOST = "INFLUX_HOST";
        //const string ENV_INFLUXDB_USERNAME = "INFLUX_USERNAME";
        //const string ENV_INFLUXDB_PASSWORD = "INFLUX_PASSWORD";
        //public static void AddInfluxDataContext(this IServiceCollection services )
        //{
        //    string url = Environment.GetEnvironmentVariable(ENV_INFLUXDB_HOST);
        //    string user = Environment.GetEnvironmentVariable(ENV_INFLUXDB_USERNAME);
        //    string pass = Environment.GetEnvironmentVariable(ENV_INFLUXDB_PASSWORD);
        //    InfluxDBClient client = new InfluxDBClient(url, user, pass);
        //    services.AddSingleton(client);
        //}

        public static void AddGraphQLObjects(this IServiceCollection services)
        {
            services.AddSingleton<MeasurementByRccType>();
            services.AddSingleton<MeasurementBySiteType>();
            services.AddSingleton<MeasurementType>();
            services.AddSingleton<MeasurementQueryType>();
            services.AddSingleton<IMeasurementDataAccess, MeasurementDataAccess>();

            services.AddSingleton<CumulativeByRccType>();
            services.AddSingleton<CumulativeBySiteType>();
            services.AddSingleton<CompareCumulativeBySiteType>();
            services.AddSingleton<AnalogSummaryType>();
            services.AddSingleton<DailyCumulateQueryType>();
            services.AddSingleton<MonthlyCumulateQueryType>();
            services.AddSingleton<ICumulativeDailyQuery, CumulativeDataAccess>();
            services.AddSingleton<ICumulativeMonthlyQuery, CumulativeDataAccess>();

            services.AddSingleton<RevenueInterface>();
            services.AddSingleton<RevenueByRccType>();
            services.AddSingleton<RevenueBySiteType>();
            services.AddSingleton<DailyRevenueQueryType>();
            services.AddSingleton<MonthlyRevenueQueryType>();
            services.AddSingleton<DailyRevenueBySummaryType>();
            services.AddSingleton<MonthlyRevenueBySummaryType>();
            services.AddSingleton<IRevenueDailyQuery, RevenueDataAccess>();
            services.AddSingleton<IRevenueMonthlyQuery, RevenueDataAccess>();

            services.AddSingleton<QueryRoot>();
            services.AddSingleton<ISchema, GraphQLSchema>();
        }

        public static HashSet<int> GetAllSiteIds(this ResolveFieldContext context)
        {
            GraphQLUserContext ctx = context.UserContext as GraphQLUserContext;
            return ctx.AllSiteIds;
        }

        public static Dictionary<int, List<int>> GetSiteKeysByRcc(this ResolveFieldContext context)
        {
            //context.ThrowIfInvalidAuthorization();
            GraphQLUserContext ctx = context.UserContext as GraphQLUserContext;
            return ctx.SiteIds;
        }

        public static bool ThrowIfInvalidAuthorization(this ResolveFieldContext context)
        {
            GraphQLUserContext ctx = context.UserContext as GraphQLUserContext;
            if(ctx.UserClaim.Identity == null || ctx.UserClaim.Identity.IsAuthenticated == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("권한이 없습니다"));
                return false;
                //throw new Exception("권한이 없습니다");
            }
            else if(ctx.UserClaim.HasClaim(x=>x.Type == GraphQLUserContext.SiteIdsByRccClaim) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상자에게는 사이트에 대한 요구(Claim)가 존재하지 않습니다"));
                return false;
            }
            ctx.UpdateClaimsValue();
            return true;
        }
    }
}
