using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public enum PmsCategoryTypes : UInt32
    {
        PMS = 1,
        ESS = 2,
        RACK = 3,
        SCHEDULE = 4,
        HOLIDAY = 5,
        CONFIGURATION = 6,
        MANUALCOMMAND = 7,
        MODECOMMAND = 8,
        PCSCOMMANDHISTORY = 9,
        TEMPERATUREANDHUMIDITY = 23,
        RACKFAULT = 101,
        EMSALIVE = 102,
        PROTECTIONCONFIG = 150
    }

    public enum PmsStateTypes : UInt32
    {
        Standby = 0,
        Active = 1,
        ConnectionError = 2
    }

    public enum LocalModeTypes : UInt32
    {
        Remote = 0,
        Local = 1
    }

    public enum OpenCloseTypes : UInt32
    {
        Open = 0,
        Close = 1
    }

    public enum PmsCurrentModeTypes : UInt32
    {
        Manual = 0,
        ScheduleMode = 1,
        ScheduleAndPeakshavingMode = 2,
        PeaskshavingMode = 3,
        IsolationMode = 4,
        LoadpredictMode = 5
    }

    public enum PmsCpMode : UInt32
    {
        CC = 0,
        CP = 1
    }
}
