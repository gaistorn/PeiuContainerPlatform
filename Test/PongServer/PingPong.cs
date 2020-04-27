using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PongServer
{
    public static class PingPong
    {
        public static readonly byte[] PING_SIGNAL_FLAG = { 0x01, 0x09, 0x08, 0x03 };
        public static readonly byte[] PONG_SIGNAL_FLAG = { 0x00, 0x03, 0x00, 0x08 };
       
    }
}
