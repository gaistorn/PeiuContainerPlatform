using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform
{
    public class PCSStatus
    {
        public const uint COMM_ERROR = 1;
        public const uint STOP = 2;
        public const uint READY = 4;
        public const uint CHARGE = 8;
        public const uint DISCHARGE = 16;
        public const uint WARNING = 32;
        public const uint FAULT = 64;
        public const uint ISOLATION = 128;
        public const uint INIT = 256;
        public const uint DG_CONNECTED = 512;
        public const uint CVCF = 1024;
        public const uint SYSTEM_STOP = 2048;
    }
}
