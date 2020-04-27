using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class CumulativeBySite : AnalogSummary
    {
        public int siteid { get; set; }
    }

    //public class CumulativeBySiteType : 

    public class CumulativeBySiteType : ObjectGraphType<CumulativeBySite>
    {
        public CumulativeBySiteType(ICumulativeDailyQuery query)
        {
            Name = "CumulativeBySite";
            Field(f => f.siteid).Description("site 번호");
            Field(f => f.sumofcharge).Description("누적 충전량");
            Field(f => f.sumofdischarge).Description("누적 방전량");
            Field(f => f.sumofpvgeneration).Description("누적 발전량");
        }
    }

    public class CompareCumulativeBySite
    {
        public int siteid { get; set; }
        public double srcsumofcharging { get; set; }
        public double destsumofcharging { get; set; }
        public double degreeofcharging { get; set; }
        public double srcsumofdischarging { get; set; }
        public double destsumofdischarging { get; set; }
        public double degreeofdischarging { get; set; }
        public double srcsumofpvpower { get; set; }
        public double destsumofpvpower { get; set; }
        public double degreeofpvpower { get; set; }
    }

    public class CompareCumulativeBySiteType : ObjectGraphType<CompareCumulativeBySite>
    {
        public CompareCumulativeBySiteType(ICumulativeDailyQuery query)
        {
            Name = "CompareCumulativeBySite";
            Field(f => f.siteid).Description("site 번호");
            Field(f => f.srcsumofcharging).Description("비교일 누적 충전량");
            Field(f => f.srcsumofdischarging).Description("비교일 누적 방전량");
            Field(f => f.srcsumofpvpower).Description("비교일 누적 발전량");

            Field(f => f.destsumofcharging).Description("대상일 누적 충전량");
            Field(f => f.destsumofdischarging).Description("대상일 누적 방전량");
            Field(f => f.destsumofpvpower).Description("대상일 누적 발전량");

            Field(f => f.degreeofcharging).Description("비교  누적 충전량");
            Field(f => f.degreeofdischarging).Description("비교  누적 방전량");
            Field(f => f.degreeofpvpower).Description("비교  누적 발전량");
        }
    }
}
