using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Power21.Device.Dao
{
    public class ModbusConfig
    {
        [JsonProperty("Protocol")]
        public ProtocolType ProtocolType { get; set; } = ProtocolType.Tcp;

        public string Address { get; set; } = "127.0.0.1";

        public ushort Port { get; set; } = 502;

        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        
    }

    public enum FunctionCode : ushort
    {
        ReadCoil = 1,
        ReadDiscreteInput = 2,
        ReadHoldingRegisters = 3,
        ReadInputRegisters = 4
    }


}
