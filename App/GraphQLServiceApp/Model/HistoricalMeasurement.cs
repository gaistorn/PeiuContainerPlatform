using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Model
{
    public class HistoricalMeasurement

    {
        public int[] unixtimestamp { get; set; }
        public double[] socs { get; set; }
        public double[] activepowers { get; set; }

        public double[] pvpowers { get; set; }
    }

   
}
