using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class CumulativeByRcc : AnalogSummary
    {
        public int rcc { get; set; }
        public IEnumerable<CumulativeBySite> sites { get; set; }
    }

    public class CumulativeByRccType : ObjectGraphType<CumulativeByRcc>
    {
        public CumulativeByRccType(ICumulativeDailyQuery query)
        {
            Name = "CumulativeByRcc";
            Field(f => f.rcc).Description("rcc 번호");
            Field(f => f.sumofcharge).Description("누적 충전량");
            Field(f => f.sumofdischarge).Description("누적 방전량");
            Field(f => f.sumofpvgeneration).Description("누적 발전량");
        }
    }
}
