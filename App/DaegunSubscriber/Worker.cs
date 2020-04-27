using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeiuPlatform.App;
using PeiuPlatform.Lib;
using PeiuPlatform.Models;

namespace DaegunSubscriber
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        readonly IPacketQueue packetQueue;
        readonly EventPublisherWorker _worker;
        PeiuPublishWorker peiuPublishWorker;
        public Worker(ILogger<Worker> logger, IPacketQueue packetQueue, EventPublisherWorker eventPublisherWorker)
        {
            _logger = logger;
            this.packetQueue = packetQueue;
            this._worker = eventPublisherWorker;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            peiuPublishWorker = new PeiuPublishWorker(stoppingToken);
            peiuPublishWorker.Initialize();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                DaegunPacket pacekt = await packetQueue.DequeueAsync(stoppingToken);
                await Task.Delay(100, stoppingToken);
            }
        }

        private async Task PublishAsync(DaegunPacket packet, CancellationToken stoppingToken)
        {
            string pcs_string = null;
            string bms_string = null;
            string pv_string = null;
            if (MqttModelConvert.ConvertPcs(packet, DateTime.Now, ref pcs_string, ref bms_string, ref pv_string))
            {
                //await base.p
                await peiuPublishWorker.publish(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, pcs_string);
                await peiuPublishWorker.publish(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, bms_string);
                await peiuPublishWorker.publish(packet.sSiteId, DeviceTypes.PV, packet.Pv.PmsIndex, pv_string);

                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, 27, (ushort)packet.Pcs.Faults[0], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, 28, (ushort)packet.Pcs.Faults[1], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, 29, (ushort)packet.Pcs.Faults[2], stoppingToken);


                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 23, (ushort)packet.Bsc.Warning[0], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 24, (ushort)packet.Bsc.Warning[1], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 25, (ushort)packet.Bsc.Warning[2], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 26, (ushort)packet.Bsc.Warning[3], stoppingToken);

                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 27, (ushort)packet.Bsc.Faults[0], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 28, (ushort)packet.Bsc.Faults[1], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 29, (ushort)packet.Bsc.Faults[2], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 30, (ushort)packet.Bsc.Faults[3], stoppingToken);

            }
        }
    }
}
