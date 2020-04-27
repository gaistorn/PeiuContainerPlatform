using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class MeasurementByRcc : Measurement
    {
        public int rcc { get; set; }
        public IEnumerable<MeasurementBySite> sites { get; set; }
    }

    public class MeasurementByRccType : ObjectGraphType<MeasurementByRcc>
    {
        public MeasurementByRccType(ICumulativeDailyQuery query)
        {
            Name = "MeasurementByRcc";
            Field(f => f.rcc).Description("rcc 번호");
            Field(f => f.countofbms).Description("BMS 갯수");
            Field(f => f.countofpcs).Description("PCS 갯수");
            Field(f => f.countofpv).Description("PV 갯수");
            Field(f => f.countofevent).Description("Active Event 갯수");
            Field(f => f.countofsites).Description("Site 갯수");
            Field(f => f.sumofcharge).Description("충전");
            Field(f => f.sumofdischarge).Description("방전");
            Field(f => f.sumofactivepower).Description("총 유효출력");
            Field(f => f.sumofpvgeneration).Description("총 발전량");
            Field(f => f.meanofsoc).Description("평균 SOC");
        }
    }
}
