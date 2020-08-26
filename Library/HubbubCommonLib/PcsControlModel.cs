using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Hubbub
{
    public class PcsControlModelBase
    {

    }
    public class PcsControlModel
    {
        public int siteid { get; set; }
        public int devicetype { get; set; }
        public int deviceindex { get; set; }
        public string timestamp { get; set; }
        public string userid { get; set; }
        public bool? StopRun { get; set; }
        public bool? FaultReset { get; set; }
        public bool? EmergencyStop { get; set; }
        public bool? ManualAuto { get; set; }
        public bool? LocalRemote { get; set; }
        public float? ActivePower { get; set; }
        public float? ReactivePower { get; set; }
        public float? SOCUpper { get; set; }
        public float? SOCLower { get; set; }
    }
}
