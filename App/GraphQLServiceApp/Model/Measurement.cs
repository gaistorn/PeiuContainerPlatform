using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.App.Model
{
    public class Measurement : AnalogSummary
    {

        public double meanofsoc { get; set; }
        public int countofpcs { get; set; }
        public int countofbms { get; set; }
        public int countofpv { get; set; }

        public int countofevent { get; set; }
        public double sumofactivepower { get; set; }

        public int countofsites { get; set; }


    }

    public class MeasurementType : ObjectGraphType<Measurement>
    {
        public MeasurementType(IMeasurementDataAccess query)
        {
            Name = "Measurement";
            Field(f => f.countofbms).Description("BMS 갯수");
            Field(f => f.countofpcs).Description("PCS 갯수");
            Field(f => f.countofpv).Description("PV 갯수");
            Field(f => f.countofevent).Description("Active Event 갯수");
            Field(f => f.sumofcharge).Description("충전");
            Field(f => f.sumofdischarge).Description("방전");
            Field(f => f.sumofactivepower).Description("총 유효출력");
            Field(f => f.sumofpvgeneration).Description("총 발전량");
            Field(f => f.meanofsoc).Description("평균 SOC");
            Field(f => f.countofsites).Description("Site 갯수");
            
        }
    }
}
