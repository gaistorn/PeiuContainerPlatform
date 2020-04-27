using NetMQ.Sockets;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device
{
    public static class HeartbeatFactory
    {
        public static void AwatingActiveServer(ArgumentParser arguements, CancellationToken cancellationToken)
        {
            using(var runTime = new NetMQRuntime())
            {
                runTime.Run(AwaitTask(arguements, cancellationToken));
            }
        }
        private static async Task AwaitTask(ArgumentParser arguements, CancellationToken cancellationToken)
        {
            using (var subSocket = new SubscriberSocket())
            {
                var logger = NLog.LogManager.GetLogger("");
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect(arguements.LookupServer);
                subSocket.Subscribe("hubbub");
                bool IsPassive = false;
                logger.Info("Searching active worker...");
                DateTime dt = DateTime.Now;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (subSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(arguements.TimeoutMS), out string topic))
                    {
                        (byte[] buffer, bool IsOk) result = await subSocket.ReceiveFrameBytesAsync(cancellationToken);
                        int idx = BitConverter.ToInt32(result.buffer, 0);
                        if(IsPassive == false)
                        {
                            logger.Info("Wait for the active worker to shutdown...[CURRENT MODE:PASSIVE MODE]");
                            IsPassive = true;
                        }
                        //Console.WriteLine("...");
                        continue;
                    }
                    else
                    {
                        logger.Warn("The active worker is terminated (or shutdown) and changes the current mode to active");
                        Console.WriteLine("[CURRENT MODE: ACTIVE MODE]");
                        break;

                    }
                }
                //(string msg, bool isOK) result = await subSocket.ReceiveFrameStringAsync(cancellationToken);
                //Console.WriteLine("Received Message: " + result.msg);
            }
        }
    }
}
