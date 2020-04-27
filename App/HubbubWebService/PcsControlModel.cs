using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class PcsControlModel
    {
        public bool? StopRun { get; set; }
        public bool? FaultReset { get; set; }
        public bool? EmergencyStop { get; set; }
        public bool? ManualAuto { get; set; }
        public float? ActivePower { get; set; }
        public float? ReactivePower { get; set; }
        public float? SOCUpper { get; set; } 
        public float? SOCLower { get; set; } 
    }
}
