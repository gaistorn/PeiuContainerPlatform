using Newtonsoft.Json.Linq;
using Power21.Device.Dao;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device.Services
{
    public interface IMqttQueue
    {
        void QueueBackgroundWorkItem((MapControl, IComparable) workItem);
        Task<(MapControl, IComparable)> DequeueAsync(TimeSpan timeout, CancellationToken cancellationToken);
    }

    public class MqttQueue : IMqttQueue
    {
        private ConcurrentQueue<(MapControl, IComparable)> _workItems =
        new ConcurrentQueue<(MapControl, IComparable)>();

        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public MqttQueue()
        {
            //var factory = new ModbusFactory();
            //modbusMaster = factory.CreateMaster()
        }

        public async Task<(MapControl, IComparable)> DequeueAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(timeout, cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;

        }

        public void QueueBackgroundWorkItem((MapControl, IComparable) workItem)
        {
            //   DaegunPacket workItem = new DaegunPacket(client, packetStruct);
            _workItems.Enqueue(workItem);
            _signal.Release();

        }
    }
}
