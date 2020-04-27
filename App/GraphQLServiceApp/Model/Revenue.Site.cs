using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class RevenueBySite : RevenueModel
    {
        public int siteid { get; set; }
    }

    public class RevenueBySiteType : ObjectGraphType<RevenueBySite>
    {
        public RevenueBySiteType(IRevenueDailyQuery query)
        {
            Name = "RevenueBySite";
            Field(f => f.siteid).Description("site 번호");
            Field(f => f.sumofmoney).Description("누적 수익금");

            //Func<ResolveFieldContext, int, object> GetDailyRevenueBySite = (context, rcc) => query.DailyRevenueByRcc(context, rcc);

            Interface<RevenueInterface>();
        }
    }
}
