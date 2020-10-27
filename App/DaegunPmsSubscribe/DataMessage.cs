using MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform
{
    public class DataMessage
    {
        public int Category { get; set; } // 0 : PCS, 1 : BAT
        public string LoggerNameByCategory
        {
            get
            {
                switch(Category)
                {
                    case 0:
                        return "pcs_logger";
                    case 1:
                        return "bat_logger";
                    case 5:
                        return "dc_logger";
                    case 6:
                        return "temp_logger";
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
