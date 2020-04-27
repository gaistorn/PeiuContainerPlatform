using GraphQL.Types;
using GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using PEIU.Models.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using PEIU.Models.DatabaseModel.Type;

namespace PeiuPlatform.App
{
    public static class GraphQLFactoryExtension
    {
        public static void RegistPeiuGraphQLModel(this IServiceCollection services)
        {
            services.AddSingleton<MeasurementData>();
            services.AddSingleton<CumulativeData>();
            services.AddSingleton<RevenueData>();
            services.AddSingleton<Query>();
            services.AddSingleton<SummryCumulativeType>();
            services.AddSingleton<SiteMeasurementType>();
            services.AddSingleton<DailyCumulativeBySiteType>();
            services.AddSingleton<DailyRevenueBySiteType>();
            services.AddSingleton<DailyRevenueBySummaryType>();
            services.AddSingleton<MonthlyCumulativeByRccType>();
            services.AddSingleton<MonthlyCumulativeBySiteType>();
            services.AddSingleton<RegionMeasurementType>();
            services.AddSingleton<CumulativeInterface>();
            services.AddSingleton<CumulativeBySiteInterface>();
            services.AddSingleton<CumulativeByRccInterface>();
            services.AddSingleton<RevenueInterface>();
            services.AddSingleton<RevenueBySiteInterface>();
            services.AddSingleton<RevenueInterface>();
            services.AddSingleton<MeasurementInterface>();
            services.AddSingleton<ISchema, MeasurementSchema>();
            services.AddHttpContextAccessor();
            services.AddGraphQL(_ =>
            {
                _.EnableMetrics = true;
#if DEBUG
                _.ExposeExceptions = true;
#else
                _.ExposeExceptions = false;
#endif
            }).AddUserContextBuilder(httpContext => new GraphQLUserContext { User = httpContext.User });
        }

        public static void UsePeiuGraphQLModel(this IApplicationBuilder app)
        {
            app.UseGraphQL<ISchema>();
            app.UseGraphQLPlayground();
        }
    }

}
