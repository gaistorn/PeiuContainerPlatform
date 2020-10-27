using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PeiuPlatform.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ST_BSC
    {
        public short SOC;
        public short SOH;
        public short DCVoltage;
        public short DCCurrent;
        public short ChargingCurrentLimit;
        public short DischargingCurrentLimit;
        public short ChargingPowerLimit;
        public short DischargingPowerLimit;
        public short MaxCellVoltage;
        public short MinCellVoltage;
        public short MaxModuleTemp;
        public short MinModuleTemp;
        public short ContainerTemp;
        public short RelayClose;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ST_PCS
    {
        public short Frequency;
        public short ActivePower;
        public short ReactivePower;
        public short PF;
        public short Crate;
        public short Prate;
        public short Feedback;
        public short SOCMax;
        public short SOCMin;
        public short Temp1;
        public short Temp2;
        public short Temp3;
        public short Temp4;
        public short AccumCharging;
        public short AccumDischarging;
        public RSTLineShort ACLineVoltage;
        public RSTPhaseShort ACPhaseVoltage;
        public RSTPhaseShort ACPhaseCurrent;
        public short DCLinkVoltage;
        public short DCVoltage;
        public short DCCurrent;
        public short DCPower;
        public RSTPhaseShort PCSPhaseVoltage;
        
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RSTLineShort
    {
        public short RS;
        public short ST;
        public short TR;

        public override string ToString()
        {
            return base.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RSTPhaseShort
    {
        public short R;
        public short S;
        public short T;
    }


}
