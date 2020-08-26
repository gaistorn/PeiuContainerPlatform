using System;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    public class HubbubInformation
    {
        public static HubbubInformation GlobalHubbubInformation { get; set; }
        public int hubbubid { get; set; }
        public bool use_zookeeper { get; set; }
        public int zookeeper_timeout { get; set; } = 10000;
        public string topicroot { get; set; }
        public bool use_packet_snapshot { get; set; } = false;
        public int rcc { get; set; }
        public int injeju
        {
            get
            {
                return rcc == 16 ? 1 : 0;
            }
        }
        
        public string ZookeeperHost { get; set; }

    }
}
