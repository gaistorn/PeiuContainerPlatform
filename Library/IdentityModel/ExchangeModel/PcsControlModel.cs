using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class PcsControlModel : ControlModelBase
    {
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

    public class ModbusControlModel : ControlModelBase
    {
        public ModbusCommandCodes commandcode { get; set; }
        public float? commandvalue { get; set; }
    }

    public enum ModbusCommandCodes : int
    {
        RUN = 1000,
        STOP = 1001,
        STANDBY = 1002,
        CHARGE = 1003,
        DISCHARGE = 1004,
        EMERGENCY_STOP = 1005,
        RESET = 1006,
        RELAY_CLOSE = 1007,
        RELAY_OPEN = 1008,
        LIMIT_SOC_MIN = 1009,
        LIMIT_SOC_MAX = 1010,
        MANUAL_MODE = 1011,
        AUTO_MODE = 1012,
        LOCAL_MODE = 1013,
        REMOTE_MODE = 1014,
        ACTIVE_POWER = 1015
    }

    public class ControlModelBase
    {
        public int siteid { get; set; }
        public int devicetype { get; set; }
        public int deviceindex { get; set; }
        public string utctimestamp { get; set; }
        public string localtimestamp { get; set; }
        public string userid { get; set; }
    }
}
