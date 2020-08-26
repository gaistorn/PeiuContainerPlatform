using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Hubbub
{
    public class HubbubInformation
    {
        public int SiteId { get; set; }
        public int ScanRateMS { get; set; } = 1000;
        public string ModbusSlaveIp { get; set; }
        public int ModbusSlavePort { get; set; }
        public int TryConnectTimeMS { get; set; } = 5000;
        public int DatabaseWriteRateMS { get; set; } = 1000;
    }
}
