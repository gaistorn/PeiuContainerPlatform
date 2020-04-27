using FireworksFramework.Mqtt;
using PeiuPlatform.App;
using PeiuPlatform.Lib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventPusherTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            EventPublisherWorker worker = new EventPublisherWorker(1);
            worker.Initialize();
            CancellationTokenSource tk = new CancellationTokenSource();
            while (true)
            {
                Console.Write("Input the Event Code: ");
                string code = Console.ReadLine();
                if (ushort.TryParse(code, out ushort EventCode))
                {
                    Task t = worker.UpdateDigitalPoint(6, DeviceTypes.BMS, 1, 40132, EventCode, tk.Token);
                    t.Wait();
                }
            }
        }
    }
}
