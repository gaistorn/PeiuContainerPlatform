using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.App
{
    public static class DeviceTypes
    {
        public const int PCS = 0;
        public const int BMS = 1;
        public const int PV = 2;

        public static string DeviceName(int DeviceType)
        {
            if (DeviceType == DeviceTypes.PCS)
                return "PCS";
            else if (DeviceType == DeviceTypes.BMS)
                return "BMS";
            else if (DeviceType == DeviceTypes.PV)
                return "PV";
            else
            {
                return "UNKNOWN(" + DeviceType + ")";
            }
        }
    }
}
