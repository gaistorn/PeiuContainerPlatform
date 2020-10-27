using Microsoft.Extensions.Logging;
using PEIU.Hubbub;
using PeiuPlatform.Lib;
using PeiuPlatform.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class ConsumeDataService : BackgroundHostService
    {
        private PeiuPublishWorker PeiuPublishWorker;
        private IPacketQueue queue;
        private EventPublisherWorker _worker;
        readonly ILogger<ConsumeDataService> logger;


        public ConsumeDataService(IPacketQueue packetQueue, EventPublisherWorker eventPublisherWorker, ILogger<ConsumeDataService> logger)
        {
            queue = packetQueue;
            _worker = eventPublisherWorker;
            this.logger = logger;
        }

        

        private async Task PublishAsync(DaegunPacket packet, CancellationToken stoppingToken)
        {
            string pcs_string = null;
            string bms_string = null;
            string pv_string = null;
            if (MqttModelConvert.ConvertPcs(packet, DateTime.Now, ref pcs_string, ref bms_string, ref pv_string))
            {
                //await base.p
                await PeiuPublishWorker.publish(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, pcs_string);
                await PeiuPublishWorker.publish(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, bms_string);
                await PeiuPublishWorker.publish(packet.sSiteId, DeviceTypes.PV, packet.Pv.PmsIndex, pv_string);

                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, 2, 27, (ushort)packet.Pcs.Faults[0], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, 2, 28, (ushort)packet.Pcs.Faults[1], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.PCS, packet.Pcs.PcsNumber, 2, 29, (ushort)packet.Pcs.Faults[2], stoppingToken);

                
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 23, (ushort)packet.Bsc.Warning[0], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 24, (ushort)packet.Bsc.Warning[1], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 25, (ushort)packet.Bsc.Warning[2], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 26, (ushort)packet.Bsc.Warning[3], stoppingToken);

                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 27, (ushort)packet.Bsc.Faults[0], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 28, (ushort)packet.Bsc.Faults[1], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 29, (ushort)packet.Bsc.Faults[2], stoppingToken);
                await _worker.UpdateDigitalPoint(packet.sSiteId, DeviceTypes.BMS, packet.Bsc.PcsIndex, 2, 30, (ushort)packet.Bsc.Faults[3], stoppingToken);

            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            PeiuPublishWorker = new PeiuPublishWorker(stoppingToken);
            PeiuPublishWorker.Initialize();
            while(true)
            {
                try
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    //if(PeiuPublishWorker.)
                    DaegunPacket packet = await queue.DequeueAsync(stoppingToken);
                    await PublishAsync(packet, stoppingToken);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
