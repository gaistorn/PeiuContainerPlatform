using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Hubbub.Model
{
    public enum PcsStatus
    {
        RUN = 1,
        STOP = 2,
        STAND_BY = 4,
        CHARGE = 8,
        DISCHARGE = 16,
        EMERGENCY = 32,
        AC_CB = 64,
        AC_MC = 128,
        DC_CB = 256,
        FAULT = 512,
        WARNING = 1024
    }
}
