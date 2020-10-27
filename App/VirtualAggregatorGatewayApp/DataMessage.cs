using MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform
{
    public class DataMessage
    {
        public int DeviceType { get; set; } // 0 : PCS, 1 : BAT
        public string DeviceName
        {
            get
            {
                switch(DeviceType)
                {
                    case 0:
                        return "PCS";
                    case 1:
                        return "BMS";
                    default:
                        return "";
                }
            }
        }
        public DateTime Timestamp { get; set; }
        public object Data { get; set; }
        public string TimestampString
        {
            get
            {
                return Timestamp.ToString("yyyyMMddHHmmss");
            }
        }

        public MqttApplicationMessage Message { get; set; }
        public MqttApplicationMessage StatusMessage { get; set; }
    }
}
