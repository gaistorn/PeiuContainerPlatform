using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TransferJejuPv
{
    public interface IPacketQueue
    {
        void QueueBackgroundWorkItem(JObject workItem);
        Task<JObject> DequeueAsync(CancellationToken cancellationToken);
    }

    public class PacketQueue : IPacketQueue
    {
        private ConcurrentQueue<JObject> _workItems =
        new ConcurrentQueue<JObject>();
        //readonly MongoDB.Driver.MongoClient client;
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        
        public PacketQueue()
        {
           
        }

        public async Task<JObject> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;

        }

        public void QueueBackgroundWorkItem(JObject workItem)
        {
         //   DaegunPacket workItem = new DaegunPacket(client, packetStruct);
            _workItems.Enqueue(workItem);
            _signal.Release();

        }
    }
}
