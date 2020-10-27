using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PeiuPlatform.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_PMS
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public PmsStateTypes PmsState;
        public LocalModeTypes LocalMode;
        public PmsCurrentModeTypes CurrentMode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] Reservesd;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_ESS
    {
        public DUT_MQTT_COMMON_HEADER Header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public DUT_MQTT_PCS[] PCS;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public DUT_MQTT_BAT[] BAT;

        public DUT_MQTT_DC DC;

        public DUT_MQTT_RPR RPR;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_RACKFAULT
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public UInt32 BankIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt32[] Warning;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt32[] Fault;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_TEMPHUMIDITY
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public UInt32 Status;
        public UInt32 DeviceLocation;
        public UInt32 DeviceIndex;
        public float Temperature;
        public float Humidity;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_RACK
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public UInt32 BankIndex;
        public UInt32 RackNumber;
        public UInt32 ModuleCount;
        public UInt32 RackStatus;
        public float SOC;
        public float SOH;
        public float DCVoltage;
        public float DCCurrent;

        public float ChargeCurrentLimit;
        public float DischargeCurrentLimit;
        public float ChargePowerLimit;
        public float DischargePowerLimit;
        public AvgMaxMinFloat CellVoltage;
        public AvgMaxMinFloat ModuleTemp;
        public MaxMinInteger CellVoltageLocation;
        public MaxMinInteger ModuleTempLocation;
        public UInt32 RelayClose;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_RPR
    {
        public Int32 ReversePowerRelayError;
        public float TotalActivePower;
        public float TotalReactivePower;
        public float TotalReverseActivePower;
        public float TotalReverseReactivePower;
        public RSTPhase PhaseVoltage;
        public RSTLine LineVoltage;
        public RSTPhase PhaseCurrent;
        public float Frequency;
        public float PowerFactor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_DC
    {
        public Int32 DemandControllerError;
        public UInt32 KepcoTimer;
        public float CurrentLoad;
        public float ForecastingPower;
        public float PreviousDemandPower;
        public float AccumulatedPower;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_PCS
    {
        public PmsCpMode CpMode;
        public LocalModeTypes LocalEnable;
        public OpenCloseTypes Ac_Magnet_Close;
        public OpenCloseTypes Dc_Magnet_Close;
        public OpenCloseTypes Grid_Sts_Status;
        public OpenCloseTypes Dgs_Sts_Status;
        public UInt32 HeartBeat;
        public UInt32 Status;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt32[] Warning;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt32[] Fault;

        public float Frequency;
        public float ActivePower;
        public float ReactivePower;
        public float PowerFactor;
        public float C_Rate;
        public float P_Rate;
        public float CommandFeedback;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public float[] Temp;
        public float TodayAccumCharge;
        public float TodayAccumDischarge;

        public RSTLine AC_LineVoltage;
        public RSTPhase AC_PhaseVoltage;
        public RSTPhase AC_PhaseCurrent;
        public float DC_LinkVoltage;
        public float DC_BatteryVoltage;
        public float DC_BatteryCurrent;
        public float DC_BatteryPower;
        public RSTPhase PCS_PhaseVoltage;
        public UInt32 OperationAvaliable;
        public UInt32 GfdDetect;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public UInt32[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_BAT
    {
        public UInt32 BatteryRackCount;
        public UInt32 BatteryStatus;
        public MaxMinInteger CellVoltageLocation;
        public MaxMinInteger ModuleTempLocation;
        public UInt32 HeartBeat;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt32[] Warning;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt32[] Fault;

        public float SOC;
        public float SOH;
        public float DCVoltage;
        public float DCCurrent;
        public float ChargeCurrentLimit;
        public float DischargeCurrentLimit;
        public float ChargePowerLimit;
        public float DischargePowerLimit;
        public MaxMinFloat CellVoltage;
        public MaxMinFloat ModuleTemp;
        public float RoomTemp;
        public UInt32 RelayClose;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public UInt32[] Reserved;

    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RSTLine
    {
        public float RS;
        public float ST;
        public float TR;

        public override string ToString()
        {
            return base.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RSTPhase
    {
        public float R;
        public float S;
        public float T;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MaxMinInteger
    {
        public UInt32 Max;
        public UInt32 Min;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AvgMaxMinFloat
    {
        public float Avg;
        public float Max;
        public float Min;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MaxMinFloat
    {
        public float Max;
        public float Min;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_COMMON_HEADER
    {
        public UInt32 Quality;
        public PmsCategoryTypes Category;
        public UInt32 PmsIndex;
        public Int64 Timestamp;
        public UInt32 Reserved1;
        public UInt32 Reserved2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_MANUALCOMMAND
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public UInt32 SequenceNumber { get; set; }
        public UInt32 Device { get; set; }
        public UInt32 SetUpFlag { get; set; }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string UserName;

        public uint Command;
        public uint ReferenceValue;
        public uint DeviceIndex;
        public uint Reversed;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_MODECOMMAND
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public UInt32 SequenceNumber { get; set; }
        public UInt32 Device { get; set; }
        public UInt32 SetUpFlag { get; set; }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string UserName;

        public uint CurrentMode;        
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_SCHEDULE
    {
        public DUT_MQTT_COMMON_HEADER Header;
        public UInt32 SequenceNumber { get; set; }
        public UInt32 Device { get; set; }
        public UInt32 SetUpFlag { get; set; }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string UserName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 210)]
        public DUT_MQTT_SCHEDULEINFO[] Schedules;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DUT_MQTT_SCHEDULEINFO
    {
        public uint ControlAction;
        public uint StartTime;
        public uint EndTime;
        public int Output;
    }



    public static class PacketParser
    {
        public static float ConvertDecimalToIEE754float(byte[] bits)
        {
            byte[] fbits = new byte[4];
            byte[] primary_bits = new byte[2];
            byte[] secondry_bits = new byte[2];
            Array.Copy(bits, primary_bits, 2);
            Array.Copy(bits, 2, secondry_bits, 0, 2);

            Array.Copy(secondry_bits, 0, fbits, 0, 2);
            Array.Copy(primary_bits, 0, fbits, 2, 2);

            float fValue = BitConverter.ToSingle(fbits, 0);
            return fValue;
        }

        public static byte[] SturctToByte(object obj)
        {
            byte[] arr = null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                Int16 size = (Int16)Marshal.SizeOf(obj);
                arr = new byte[size];
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);

            }
            catch (Exception e)
            {
                // 예외 발생
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        public static T ByteToStruct<T>(byte[] buffer) where T : struct
        {
            int iSize = Marshal.SizeOf(typeof(T));

            if (iSize > buffer.Length)
            {
                throw new Exception(string.Format("SIZE ERR(len:{0} sz:{1})", buffer.Length, iSize));
            }

            IntPtr ptr = Marshal.AllocHGlobal(iSize);
            Marshal.Copy(buffer, 0, ptr, iSize);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return obj;
        }
    }
}
