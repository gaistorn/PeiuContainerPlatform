using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    public interface IModbusHubbubTemplate
    {
        ModbusConnectionInfo ConnectionInfo { get; set; }
        ModbusHubbub Hubbub { get; set; }
    }

    public class ModbusHubbubMappingTemplate// : IHubbubAnalogMappingTemplate, IHubbubDigitalMappingTemplate
    {
        public ModbusConnectionInfo ConnectionInfo { get; set; }
        public ModbusHubbub Hubbub { get; set; }
        public IList<VwStandardPcsStatusPoint> StandardPcsStatuses { get; set; }
        public IList<VwStandardAnalogPoint> StandardAnalogPoints { get; set; }
        public IList<ModbusDigitalStatusPoint> ModbusDigitalStatusPoints { get; set; }
        public IList<VwDigitalOutputPoint> ModbusDigitalOutputPoints { get; set; }
        public IList<VwModbusInputPoint> ModbusInputPointList { get; set; }

    }

    //public class HubbubAnalogMappingTemplate : IHubbubAnalogMappingTemplate
    //{
    //    public ModbusConnectionInfo ConnectionInfo { get; set; }
    //    public ModbusHubbub Hubbub { get; set; }

    //    public AnalogInputGroup AnalogInputGroup { get; set; }
    //    public IList<VwAnalogPoint> AnalogInputPoints { get; set; }
    //}

    //public interface IHubbubAnalogMappingTemplate : IModbusHubbubTemplate
    //{
    //    AnalogInputGroup AnalogInputGroup { get; set; }
    //    IList<VwAnalogPoint> AnalogInputPoints { get; set; }
    //}

    //public interface IHubbubDigitalMappingTemplate : IModbusHubbubTemplate
    //{
    //    DigitalInputGroup DigitalInputGroup { get; set; }
    //    DigitalOutputGroup DigitalOutputGroup { get; set; }
    //    DigitalStatusGroup DigitalStatusGroup { get; set; }

    //    IList<ModbusDigitalInputPoint> DigitalInputPoints { get; set; }
    //    IList<ModbusDigitalOutputPoint> DigitalOutputPoints { get; set; }
    //    IList<ModbusDigitalStatusPoint> DigitalStatusPoints { get; set; }
    //}

    public interface IIdentifiedObject
    {
        int Id { get; set; }
    }
}
