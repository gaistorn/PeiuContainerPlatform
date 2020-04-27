using GraphQL.Types;
using PeiuPlatform.App.Model;
using PeiuPlatform.App.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class QueryRoot : ObjectGraphType<object>
    {
        public double money { get; set; }
        

        public QueryRoot()
        {
            //Func<ResolveFieldContext, object> DailyRevenueQueryFunc = (context) => GetQuery<DailyRevenueQueries>();
            //FieldDelegate<DailyRevenueQueryType>("dailyrevenues",
            //   resolve: DailyRevenueQueryFunc);

            //Func<ResolveFieldContext, object> MonthlyRevenueQueryFunc = (context) => GetQuery<MonthlyRevenueQueries>();
            //FieldDelegate<MonthlyRevenueQueryType>("monthlyrevenue",
            //   resolve: MonthlyRevenueQueryFunc);

            //Func<ResolveFieldContext, object> DailyCumulateQueryFunc = (context) => GetQuery<DailyCumulateQueries>();
            //FieldDelegate<DailyRevenueQueryType>("dailycumulate",
            //   resolve: DailyCumulateQueryFunc);

            //Func<ResolveFieldContext, object> MonthlyCumulateQueryFunc = (context) => GetQuery<MonthlyCumulateQueries>();
            //FieldDelegate<MonthlyRevenueQueryType>("monthlycumulate",
            //   resolve: MonthlyCumulateQueryFunc);

            RegisterQuery<DailyRevenueQueryType, DailyRevenueQueries>("dailyrevenues", "금일 수익금");
            RegisterQuery<MonthlyRevenueQueryType, MonthlyRevenueQueries>("monthlyrevenues", "금월 수익금");
            RegisterQuery<DailyCumulateQueryType, DailyCumulateQueries>("dailycumulate", "금일 누적");
            RegisterQuery<MonthlyCumulateQueryType, MonthlyCumulateQueries>("monthlycumulate", "금월 누적");
            RegisterQuery<MeasurementQueryType, MeasurementQuery>("measurements", "현재 취득데이터");
        }

        private void RegisterQuery<TGraphType, T>(string queryName, string description = null) where T : new() where TGraphType : IGraphType
        {
            Func<ResolveFieldContext, object> Func = (context) => GetQuery<T>();
            FieldDelegate<TGraphType>(queryName, description: description,
               resolve: Func);
        }

        private T GetQuery<T>() where T : new()
        {
            return new T();
        }
    }
}
