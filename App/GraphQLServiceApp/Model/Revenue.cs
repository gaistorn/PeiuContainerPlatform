using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public abstract class RevenueModel
    {
        public double sumofmoney { get; set; }
    }

    public class RevenueInterface : InterfaceGraphType<RevenueModel>
    {
        public RevenueInterface()
        {
            Name = "Revenue";
            Field(f => f.sumofmoney).Description("예상 수익금");
        }
    }

}
