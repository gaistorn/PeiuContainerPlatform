using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PingClient
{
    public static class PingPong
    {
        public static readonly byte[] PING_SIGNAL_FLAG = { 0x01, 0x09, 0x08, 0x03 };
        public static readonly byte[] PONG_SIGNAL_FLAG = { 0x00, 0x03, 0x00, 0x08 };
        public static bool Ping(ref string PongHost, params string[] IpAddress)
        {
            if (IpAddress.Length == 0)
                return false;
            //var logger = NLog.LogManager.GetCurrentClassLogger();
            foreach (string host in IpAddress)
            {
                using (var pingSocket = new RequestSocket($">tcp://{host}:5000"))
                {
                    byte[] buffers = null;
                    pingSocket.SendFrame(PING_SIGNAL_FLAG);
                    //logger.Info($"Attempt to send ping signal: --> [{host}]");
                    if (pingSocket.TryReceiveFrameBytes(TimeSpan.FromSeconds(3), out buffers))
                    {
                        PongHost = host;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
