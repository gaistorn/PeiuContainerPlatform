using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class RevenueBySummary : RevenueModel
    {
        public IEnumerable<RevenueModelByRcc> revenuerccs { get; set; }
    }

    public class DailyRevenueBySummaryType : ObjectGraphType<RevenueBySummary>
    {
        public DailyRevenueBySummaryType(IRevenueDailyQuery query)
        {
            Name = "DailyRevenueBySummary";
            Field(f => f.sumofmoney).Description("금일 누적수익금");
            Interface<RevenueInterface>();
        }
    }

    public class MonthlyRevenueBySummaryType : ObjectGraphType<RevenueBySummary>
    {
        public MonthlyRevenueBySummaryType(IRevenueDailyQuery query)
        {
            Name = "MonthlyRevenueBySummary";
            Field(f => f.sumofmoney).Description("월간 누적수익금");
            //Field(f => f.revenuerccs).Description("rcc 리스트들");
            Interface<RevenueInterface>();
        }
    }
}
