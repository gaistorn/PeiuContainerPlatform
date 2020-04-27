using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PeiuPlatform.Models
{
    public class DaegunPacketClass
    {
        public DaegunPacket Packet { get; private set; }
        public DateTime Timestamp { get; private set; }

        public DaegunPacketClass(DaegunPacket packet, DateTime timeStamp)
        {
            Packet = packet;
            Timestamp = timeStamp;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DaegunPacket
    {
        public short sSiteId;
        public DaegunPcsPacket Pcs;
        public DaegunBSCPacket Bsc;
        public DaegunMeterPacket Ess;
        public DaegunMeterPacket Pv;
       // public DateTime timestamp;
        //public DaegunPcsPacket Pcs;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DaegunPcsPacket
    {
        public short PcsNumber;
        public ushort Status;
        public float Frequency;
        public float ActivePower;
        public float ReactivePower;
        public float PowerFactor;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] Temp;

        public float AccumulateCharge;
        public float AccumulateDischarge;

        public RSTPacket AC_line_voltage;
        public RSTPacket AC_phase_voltage;
        public RSTPacket AC_phase_current;
        public float Dc_link_voltage;
        public float Dc_battery_voltage;
        public float Dc_battery_current;
        public float Dc_battery_power;
        public ushort HeartBeat;
        public uint Comm_fault;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Faults;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Warrning;



        public void Converter(DateTime MeasureTime, out PCSMeasurement measurement, out PCSSampleValue sample, out SensorGroupValue sensor, out StatusValue status)
        {
            measurement = new PCSMeasurement();
            measurement.MeasureTime = MeasureTime;
            measurement.ACActPwr = this.ActivePower;
            measurement.ACChgAccPwr = this.AccumulateCharge;
            measurement.ACDhgAccPwr = this.AccumulateDischarge;
            measurement.DCSrcCrt = this.Dc_battery_current;
            measurement.DCSrcPwr = this.Dc_battery_power;
            measurement.DCSrcVlt = this.Dc_battery_voltage;
            measurement.PwrFactor = this.PowerFactor;
            measurement.Frequency = this.Frequency;
            RSTPacket rst = this.AC_phase_current;
            measurement.PCSCrt = new RST(rst.R, rst.S, rst.T);
            rst = this.AC_phase_voltage;
            measurement.PCSVlt = new RST(rst.R, rst.S, rst.T);
            measurement.PCSPwr = new PairPower(this.ActivePower, this.ReactivePower);

            sensor = new SensorGroupValue();
            sensor.MeasureTime = MeasureTime;
            List<SensorValue> sensor_values = new List<SensorValue>();
            for (int i = 0; i < this.Temp.Length; i++)
            {
                sensor_values.Add(new SensorValue() { Name = $"Temp{i + 1}", Value = this.Temp[i] });
            }
            sensor.Sensors = sensor_values.ToArray();

            sample = new PCSSampleValue();
            sample.MeasureTime = MeasureTime;
            sample.Frequency = this.Frequency;
            sample.PwrFactor = this.PowerFactor;

            status = new StatusValue();
            status.MeasureTime = MeasureTime;

            List<EventValue> event_values = new List<EventValue>();
            for (int i = 0; i < this.Faults.Length; i++)
            {
                event_values.Add(new EventValue() { Name = $"Fault{i + 1}", Value = this.Faults[i] });
            }

            status.Faults = event_values.ToArray();
            event_values.Clear();
            for (int i = 0; i < this.Warrning.Length; i++)
            {
                event_values.Add(new EventValue() { Name = $"Warn{i + 1}", Value = this.Warrning[i] });
            }
            status.Warrning = event_values.ToArray();
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DaegunBSCPacket
    {
        public short PcsIndex;
        public ushort BscHeartBeat;
        public ushort TotalRacks;
        public ushort OnlineRacks;
        public ushort Status;
        public ushort RelayClose;
        public float Soc;
        public float Soh;
        public float DcVoltage;
        public float DcCurrent;
        public float ChargeCurrentLimit;
        public float DischargeCurrentLimit;
        public float ChargePowerLimit;
        public float DischargePowerLimit;
        public FloatRange CellVoltageRange;
        public FloatRange ModuleTempRange;
        public float RoomTemp;
        public ShortRange CellVoltageLocationRange;
        public ShortRange ModuleTempLocationRange;


        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Warning;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Faults;

        //public uint OtherError;

        public void Converter(DateTime MeasureTime, out BSCMeasurementValue measurement, out BSCSensorValue sensor, out StatusValue status)
        {
            measurement = new BSCMeasurementValue();
            measurement.BkDCChgCrtlmt = ChargeCurrentLimit;
            measurement.BkDCDhgCrtlmt = DischargeCurrentLimit;
            measurement.BkDCDhgPwrlmt = ChargePowerLimit;
            measurement.BkHiCellVlt = CellVoltageRange.Max;
            measurement.BkLoCellVlt = CellVoltageRange.Min;
            measurement.BkHiCellVltLoc = CellVoltageLocationRange.Max;
            measurement.BkLoCellVltLoc = CellVoltageLocationRange.Min;
            measurement.SOC = Soc;
            measurement.SOH = Soh;
            measurement.DCCrt = DcCurrent;
            measurement.DCVlt = DcVoltage;
            measurement.VltOfCellRange = new FloatingRange() { Min = CellVoltageRange.Min, Max = CellVoltageRange.Max };
            measurement.MeasureTime = MeasureTime;

            sensor = new BSCSensorValue();
            sensor.TempOfCellRange = new FloatingRange() { Min = ModuleTempRange.Min, Max = ModuleTempRange.Max };

            status = new StatusValue();
            status.Status = new EventValue[]
            {
                new EventValue(){Name = "status", Value = Status },

                new EventValue(){Name = "Fault1", Value = Faults[0] },
                new EventValue(){Name = "Fault2", Value = Faults[1] },
                new EventValue(){Name = "Fault3", Value = Faults[2] },
                new EventValue(){Name = "Fault4", Value = Faults[3] },

                new EventValue(){Name = "Warn1", Value = Faults[0] },
                new EventValue(){Name = "Warn2", Value = Faults[1] },
                new EventValue(){Name = "Warn3", Value = Faults[2] },
                new EventValue(){Name = "Warn4", Value = Faults[3] },

                //new EventValue(){Name = "OtherError", Value = OtherError },
            };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DaegunMeterPacket
    {
        public short DeviceIndex;
        public ushort PmsIndex;
        public byte CommError;
        public uint ReversePowerRelayError;
        public float TotalActivePower;
        public float TotalReactivePower;
        public float ReverseActivePower;
        public float ReverseReactivePower;
        public RSTPacket Voltage;
        public RSTPacket Current;
        public float Frequency;
        public float EnergyTotalActivePower;
        public float EnergyTotalReactivePower;
        public float EnergyTotalReverseActivePower;

        //public void Convert(DateTime MeasureTime, out PVMeasurement measure, out PVSampleValue sample, out StatusValue status)
        //{
        //    //measure = new PVMeasurement();
        //    //measure.PvVlt 
        //    //measure.PvCrt 
        //}
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShortRange
    {
        public ushort Max;
        public ushort Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IntegerRange
    {
        public uint Max;
        public uint Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FloatRange
    {
        public float Max;
        public float Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RSTPacket
    {
        public float R;
        public float S;
        public float T;

        public override string ToString()
        {
            return $"{R},{S},{T}";
        }
    }

    public class UShortRange
    {
        public ushort Max;
        public ushort Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }


    public class UInt32Range
    {
        public uint Max;
        public uint Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }

    public class FloatingRange
    {
        public static readonly FloatingRange Empty = new FloatingRange { Max = 0, Min = 0 };
        public float Max;
        public float Min;
        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }

    public class PairPower
    {
        public static readonly PairPower Empty = new PairPower(0, 0);
        public float P;
        public float Q;

        public PairPower() { }
        public PairPower(float p, float q)
        {
            P = p;
            Q = q;
        }
    }

    public class RST
    {
        public static readonly RST Empty = new RST(0, 0, 0);
        public float R;
        public float S;
        public float T;

        public RST() { }
        public RST(float r, float s, float t)
        {
            R = r;
            S = s;
            T = t;
        }

        public override string ToString()
        {
            return $"{R},{S},{T}";
        }
    }
}
