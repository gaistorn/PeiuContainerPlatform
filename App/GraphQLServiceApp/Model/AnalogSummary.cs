using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.App.Model
{
    public class AnalogSummary
    {
        public virtual double sumofcharge { get; set; }
        public virtual double sumofdischarge { get; set; }
        public double sumofpvgeneration { get; set; }
    }

    public class AnalogSummaryType : ObjectGraphType<AnalogSummary>
    {
        public AnalogSummaryType(ICumulativeDailyQuery query)
        {
            Name = "AnalogSummary";
            Field(f => f.sumofcharge).Description("누적 충전량");
            Field(f => f.sumofdischarge).Description("누적 방전량");
            Field(f => f.sumofpvgeneration).Description("누적 발전량");
        }
    }
}
