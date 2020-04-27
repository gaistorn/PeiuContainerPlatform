using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class MeasurementBySite : Measurement
    {
        public int siteid { get; set; }
    }

    //public class CumulativeBySiteType : 

    public class MeasurementBySiteType : ObjectGraphType<MeasurementBySite>
    {
        public MeasurementBySiteType(ICumulativeDailyQuery query)
        {
            Name = "MeasurementBySite";
            Field(f => f.siteid).Description("site 번호");
            Field(f => f.countofbms).Description("BMS 갯수");
            Field(f => f.countofpcs).Description("PCS 갯수");
            Field(f => f.countofpv).Description("PV 갯수");
            Field(f => f.countofevent).Description("Active Event 갯수");
            Field(f => f.sumofcharge).Description("충전");
            Field(f => f.sumofdischarge).Description("방전");
            Field(f => f.sumofactivepower).Description("총 유효출력");
            Field(f => f.sumofpvgeneration).Description("총 발전량");
            Field(f => f.meanofsoc).Description("평균 SOC");
        }
    }
}
