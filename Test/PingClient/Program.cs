using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using NetMQ;
namespace PingClient
{
    class Program
    {
        public static IList<string> allowableCommandLineArgs
            = new[] { "TopicA", "TopicB", "All" };
        static void Main(string[] args)
        {
            string st = GetStat(6306);
            Console.WriteLine(Environment.OSVersion);
            string topic = "Topic";
            Console.WriteLine("Subscriber started for Topic : {0}", topic);
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect("tcp://localhost:12345");

                subSocket.Subscribe(topic);
                Console.WriteLine("Subscriber socket connecting...");
                while (true)
                {
                    string receivedTopic = subSocket.ReceiveFrameString();
                    byte[] buffer = subSocket.ReceiveFrameBytes();
                    int idx = BitConverter.ToInt32(buffer, 0);                    
                    Console.WriteLine($"[{receivedTopic}] {idx}");
                }
            }
        }

        private static string GetStat(int stat)
        {
            if ((stat & 2) == 2)
            {
                // Running
                if ((stat & 32) == 32)
                {
                    return "충전중";
                }
                else if ((stat & 64) == 64)
                {
                    return "방전중";
                }
                else
                {
                    return "대기";
                }
            }
            else
            {
                return "정지";
            }
        }
    }
}
