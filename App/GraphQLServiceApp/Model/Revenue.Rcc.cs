using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class RevenueModelByRcc : RevenueModel
    {
        public int rcc { get; set; }
        public IEnumerable<RevenueBySite> sites { get; set; }
    }

    public class RevenueByRccType : ObjectGraphType<RevenueModelByRcc>
    {
        public RevenueByRccType(IRevenueDailyQuery query)
        {
            Name = "RevenueByRcc";

            Field(f => f.rcc).Description("rcc 번호");
            Field(f => f.sumofmoney).Description("누적 수익금");
            //Field(f => f.sites).Description("소속 Sites들");

            Interface<RevenueInterface>();
        }
    }
}
