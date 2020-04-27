using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class HistoricalMeasurementBySite : HistoricalMeasurement
    {
        public int siteid { get; set; }
    }

    public class HistoricalMeasurementBySiteType : ObjectGraphType<HistoricalMeasurementBySite>
    {
        public HistoricalMeasurementBySiteType()
        {
            Name = "HistoricalMeasurementBySite";
            Field(f => f.unixtimestamp).Description("timestamp(unix)");
            Field(f => f.activepowers).Description("유효출력들");
            Field(f => f.pvpowers).Description("PV발전들");
            Field(f => f.socs).Description("soc들");
            Field(f => f.unixtimestamp).Description("unix timestamp");
            Field(f => f.siteid).Description("siteid");

        }
    }
}
