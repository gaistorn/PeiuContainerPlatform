using System;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    public abstract class ModbusPointModel
    {
        public int FunctionCode { get; set; } = 3;
        public ushort Offset { get; set; }
        public DataTypes DataType { get; set; }
        public virtual string Name { get; set; }
        public int GroupId { get; set; }
        public int DeviceType { get; set; }
    }

    public class AnalogInputModbusPoint : ModbusPointModel
    {
        public float Scale { get; set; }
        public int ByteIndex { get; set; }
        public int AnalogCode { get; set; }
    }

    public abstract class DigitalModbusPoint : ModbusPointModel
    {
        public ushort BitMask { get; set; }
    }

    public class DigitalStatusModbusPoint : DigitalModbusPoint
    {
        public int StatusCode { get; set; }
    }

    public class DigitalInputModbusPoint : DigitalModbusPoint
    {
        public int Level { get; set; }
    }

    public class DigitalOutputModbusPoint : DigitalModbusPoint
    {
        public int CommandCode { get; set; }
    }

    public enum PointTypes
    {
        AI = 0,
        DI = 1,
        ST = 2
    }

    public enum DeviceTypes
    {
        PCS = 0,
        BMS = 1,
        PV = 2,
        PCS_STATUS = 3
    }

    public enum DataTypes
    {
        BYTE = 100,
        INT16 = 101,
        UINT16 = 102,
        INT32 = 103,
        UINT32 = 104,
        FLOAT = 105,
        DOUBLE = 106
    }
}
