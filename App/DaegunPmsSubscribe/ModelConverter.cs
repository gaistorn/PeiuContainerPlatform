using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeiuPlatform.App;
using PeiuPlatform.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform
{
    public static class ModelConverter
    {
        readonly static JObject PCSStatusSample;
        static ModelConverter()
        {
            PCSStatusSample = new JObject();
            PCSStatusSample.Add("ditype", 1);
            PCSStatusSample.Add("devicetype", 0);
            PCSStatusSample.Add("deviceindex", 0);
            PCSStatusSample.Add("RUN", 0);
            PCSStatusSample.Add("STOP", 0);
            PCSStatusSample.Add("STAND_BY", 0);
            PCSStatusSample.Add("CHARGE", 0);
            PCSStatusSample.Add("DISCHARGE", 0);
            PCSStatusSample.Add("EMERGENCY", 0);
            PCSStatusSample.Add("AC_CB", 0);
            PCSStatusSample.Add("AC_MC", 0);
            PCSStatusSample.Add("DC_CB", 0);
            PCSStatusSample.Add("FAULT", 0);
            PCSStatusSample.Add("WARNING", 0);
            PCSStatusSample.Add("COMM", 0);
            PCSStatusSample.Add("LOCALREMOTE", 0);
            PCSStatusSample.Add("MANUALAUTO", 0);
        }

        public static DataMessage CreateMessage(object data, int Category, DateTime Timestamp)
        {
            DataMessage dataMessage = new DataMessage();
            dataMessage.Category = Category;
            dataMessage.Data = data;
            dataMessage.Timestamp = Timestamp;
            return dataMessage;
        }

        public static DataMessage CreateMessage(string message, object data, int Category, string topicName, DateTime Timestamp)
        {
            return CreateMessage(message, data, Category, topicName, null, null, Timestamp);
        }

        public static DataMessage CreateMessage(string message, object data, int Category, string topicName, JObject status, string status_topic, DateTime Timestamp)
        {
            DataMessage dataMessage = new DataMessage();
            dataMessage.Category = Category;
            dataMessage.Data = data;
            dataMessage.Timestamp = Timestamp;

            
            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(message);
            dataMessage.Message = new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithPayload(payload_buffer)
                .WithTopic(topicName)
                .Build();

            if (status != null)
            {
                payload_buffer = System.Text.Encoding.UTF8.GetBytes(status.ToString());
                dataMessage.StatusMessage = new MqttApplicationMessageBuilder()
                    .WithAtLeastOnceQoS()
                    .WithPayload(payload_buffer)
                    .WithTopic(status_topic)
                    .Build();
            }
            return dataMessage;
        }

        public static MqttApplicationMessage CreateMessage(string message, string topicName)
        {
            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(message);
            return new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithPayload(payload_buffer)
                .WithTopic(topicName)
                .Build();
        }

        public static void EventProcessing(IPacketSubscriber eventPublisher, DUT_MQTT_ESS packet)
        {
            DUT_MQTT_PCS[] pcsPackets = packet.PCS;
            DUT_MQTT_BAT[] bmsPackets = packet.BAT;

            int siteid = ENV.SiteId;
            int rccid = ENV.RccId;
            DateTime timestamp = DateTime.FromFileTime(packet.Header.Timestamp);
            for (int pcsIdx = 0; pcsIdx < ENV.PcsCount; pcsIdx++)
            {
                DUT_MQTT_PCS pcsPacket = pcsPackets[pcsIdx];
                DUT_MQTT_BAT batPacket = bmsPackets[pcsIdx];
                PCSEventProcessing(eventPublisher, pcsIdx + 1, pcsPacket, packet.Header.Timestamp);
                BatEventProcessing(eventPublisher, pcsIdx + 1, batPacket, packet.Header.Timestamp);

            }
        }

        private static JObject CreatePcsStatusModel(int deviceIndex, DUT_MQTT_PCS pcs)
        {
            JObject pcs_obj = (JObject)PCSStatusSample.DeepClone();
            pcs_obj["deviceindex"] = deviceIndex;
            pcs_obj["RUN"] = BitWise(pcs.Status, PCSStatus.STOP, true);
            pcs_obj["STOP"] = BitWise(pcs.Status, PCSStatus.STOP);
            pcs_obj["STAND_BY"] = BitWise(pcs.Status, PCSStatus.READY);
            pcs_obj["CHARGE"] = BitWise(pcs.Status, PCSStatus.CHARGE);
            pcs_obj["DISCHARGE"] = BitWise(pcs.Status, PCSStatus.DISCHARGE);
            pcs_obj["EMERGENCY"] = BitWise(pcs.Status, PCSStatus.SYSTEM_STOP);
            pcs_obj["AC_CB"] = pcs.Ac_Magnet_Close == OpenCloseTypes.Close ? 1 : 0;
            pcs_obj["AC_MC"] = pcs.Grid_Sts_Status == OpenCloseTypes.Close ? 1 : 0;
            pcs_obj["DC_CB"] = pcs.Dc_Magnet_Close == OpenCloseTypes.Close ? 1 : 0;
            pcs_obj["FAULT"] = BitWise(pcs.Status, PCSStatus.FAULT);
            pcs_obj["WARNING"] = BitWise(pcs.Status, PCSStatus.WARNING);
            pcs_obj["COMM"] = BitWise(pcs.Status, PCSStatus.COMM_ERROR);
            pcs_obj["LOCALREMOTE"] = pcs.LocalEnable == LocalModeTypes.Remote ? 1 : 0;
            return pcs_obj;
        }

        private static int BitWise(IComparable value, uint BitFlag, bool IsReverse = false)
        {
            bool result = (((dynamic)value & BitFlag) == BitFlag);
            if (IsReverse)
                return result == false ? 1 : 0;
            else
                return result ? 1 : 0;
        }

        private static void BatEventProcessing(IPacketSubscriber eventPublisher, int deviceIndex, DUT_MQTT_BAT bat, long timestamp)
        {
            eventPublisher.UpdateDigitalPoint(ENV.SiteId, 1, deviceIndex, 3, 30001, bat.Warning[0], timestamp);
            eventPublisher.UpdateDigitalPoint(ENV.SiteId, 1, deviceIndex, 3, 40001, bat.Fault[0], timestamp);
        }

        private static void PCSEventProcessing(IPacketSubscriber eventPublisher, int deviceIndex, DUT_MQTT_PCS pcs, long timestamp)
        {
            eventPublisher.UpdateDigitalPoint(ENV.SiteId, 0, deviceIndex, 3, 10001, pcs.Warning[0], timestamp);
            eventPublisher.UpdateDigitalPoint(ENV.SiteId, 0, deviceIndex, 3, 10002, pcs.Warning[1], timestamp);

            for (int i = 0; i < 5; i++)
            {
                eventPublisher.UpdateDigitalPoint(ENV.SiteId, 0, deviceIndex, 3, 20001 + i, pcs.Fault[i], timestamp);
            }
        }

        public static void DataProcessing(DUT_MQTT_TEMPHUMIDITY packet, ConcurrentQueue<DataMessage> queue)
        {
            if (packet.Status != 0)
                return;
            int siteid = ENV.SiteId;
            int rccid = ENV.RccId;

            string dc_topic_name = $"hubbub/{siteid}/DC/AI";
            DateTime timestamp = DateTime.FromFileTime(packet.Header.Timestamp);
            string timestamp_string = timestamp.ToString("yyyyMMddHHmmss");

            queue.Enqueue(CreateMessage(packet, 6, timestamp));
        }

        public static void DataProcessing(DUT_MQTT_ESS packet, ConcurrentQueue<DataMessage> queue)
        {
            DUT_MQTT_PCS[] pcsPackets = packet.PCS;
            DUT_MQTT_BAT[] bmsPackets = packet.BAT;
            DUT_MQTT_DC dcPacket = packet.DC;
            
            int siteid = ENV.SiteId;
            int rccid = ENV.RccId;

            string dc_topic_name = $"hubbub/{siteid}/DC/AI";
            DateTime timestamp = DateTime.FromFileTime(packet.Header.Timestamp);
            //DateTime timestamp_utc = DateTime.FromFileTimeUtc(packet.Header.Timestamp);

            /// UTC 시간으로 변경
            string timestamp_string = timestamp.AddHours(-18).ToString("yyyyMMddHHmmss");
            for (int pcsIdx = 0; pcsIdx < ENV.PcsCount; pcsIdx++)
            {
                DUT_MQTT_PCS pcsPacket = pcsPackets[pcsIdx];
                DUT_MQTT_BAT batPacket = bmsPackets[pcsIdx];
                int pcsNo = pcsIdx + 1;

                string pcs_topic_name = $"hubbub/{siteid}/PCS{pcsNo}/AI";
                string pcs_status_topic_name = $"hubbub/{siteid}/0/{pcsNo}/stat";
                string bat_topic_name = $"hubbub/{siteid}/BMS{pcsNo}/AI";

                JObject obj = CreateTemporary(1, "PCS_SYSTEM", $"PCS{pcsNo}", siteid, rccid, timestamp_string);
                JObject pcs_obj = CreatePcsStatusModel(pcsNo, pcsPacket);
                obj.Add("freq", pcsPacket.Frequency);
                obj.Add("acGridVlt", 0);
                obj.Add("acGridCrtLow", 0);
                obj.Add("acGridCrtHigh", 0);
                obj.Add("acGridPwr", 0);
                obj.Add("actPwrKw", pcsPacket.ActivePower);
                obj.Add("rctPwrKw", pcsPacket.ReactivePower);
                obj.Add("pwrFact", pcsPacket.PowerFactor);
                obj.Add("acGridVltR", pcsPacket.AC_PhaseVoltage.R);
                obj.Add("acGridVltS", pcsPacket.AC_PhaseVoltage.S);
                obj.Add("acGridVltT", pcsPacket.AC_PhaseVoltage.T);
                obj.Add("acGridCrtR", pcsPacket.AC_PhaseCurrent.R);
                obj.Add("acGridCrtS", pcsPacket.AC_PhaseCurrent.S);
                obj.Add("acGridCrtT", pcsPacket.AC_PhaseCurrent.T);
                obj.Add("actCmdLimitLowChg", 0);
                obj.Add("actCmdLimitLowDhg", 0);
                obj.Add("actCmdLimitHighChg", batPacket.ChargePowerLimit);
                obj.Add("actCmdLimitHighDhg", batPacket.DischargePowerLimit);
                queue.Enqueue(CreateMessage(obj.ToString(), pcsPacket, 0, pcs_topic_name, pcs_obj, pcs_status_topic_name, timestamp));

                obj = CreateTemporary(2, "PCS_BATTERY", $"BMS{pcsNo}", siteid, rccid, timestamp_string);
                obj.Add("bms_soc", batPacket.SOC);
                obj.Add("bms_soh", batPacket.SOH);
                obj.Add("dcCellPwr", pcsPacket.DC_BatteryPower);
                obj.Add("dcCellVlt", pcsPacket.DC_BatteryVoltage);
                obj.Add("dcCellCrt", pcsPacket.DC_BatteryCurrent);
                obj.Add("dcCellTmpMx", batPacket.ModuleTemp.Max);
                obj.Add("dcCellTmpMn", batPacket.ModuleTemp.Min);
                obj.Add("dcCellVltMx", batPacket.CellVoltage.Max);
                obj.Add("dcCellVltMn", batPacket.CellVoltage.Min);
                queue.Enqueue(CreateMessage(obj.ToString(), batPacket, 1, bat_topic_name, timestamp));               
            }
            if (dcPacket.DemandControllerError != 0)
                return;
            JObject dc_obj = CreateTemporary(5, "DC", $"DC", siteid, rccid, timestamp_string);
            dc_obj.Add("KepcoTimer", dcPacket.KepcoTimer);
            dc_obj.Add("CurrentLoad", dcPacket.CurrentLoad);
            dc_obj.Add("ForecastingPower", dcPacket.ForecastingPower);
            dc_obj.Add("PreviousDemandPower", dcPacket.PreviousDemandPower);
            dc_obj.Add("AccumulatedPower", dcPacket.AccumulatedPower);
            queue.Enqueue(CreateMessage(dc_obj.ToString(), dcPacket, 5, dc_topic_name, timestamp));
        }


        public static bool TryConvertPcs(DUT_MQTT_ESS packet, out DataMessage[] pcs_messages,
            out DataMessage[] bat_messages, out DataMessage dc_message)
        {
            pcs_messages = new DataMessage[ENV.PcsCount];
            bat_messages = new DataMessage[ENV.PcsCount];
            dc_message = new DataMessage();
            try
            {
                DUT_MQTT_PCS[] pcsPackets = packet.PCS;
                DUT_MQTT_BAT[] bmsPackets = packet.BAT;
                DUT_MQTT_DC dcPacket = packet.DC;
                int siteid = ENV.SiteId;
                int rccid = ENV.RccId;

                DateTime timestamp = DateTime.Now;
                string timestamp_string = timestamp.ToString("yyyyMMddHHmmss");
                string dc_topic_name = $"hubbub/{siteid}/DC/AI";

                for (int pcsIdx = 0; pcsIdx < ENV.PcsCount; pcsIdx++)
                {
                    DUT_MQTT_PCS pcsPacket = pcsPackets[pcsIdx];
                    DUT_MQTT_BAT batPacket = bmsPackets[pcsIdx];
                    
                    int pcsNo = pcsIdx + 1;

                    string pcs_topic_name = $"hubbub/{siteid}/PCS{pcsNo}/AI";
                    string bat_topic_name = $"hubbub/{siteid}/BMS{pcsNo}/AI";

                    JObject obj = CreateTemporary(1, "PCS_SYSTEM", $"PCS{pcsNo}", siteid, rccid, timestamp_string);
                    obj.Add("freq", pcsPacket.Frequency);
                    obj.Add("acGridVlt", 0);
                    obj.Add("acGridCrtLow", 0);
                    obj.Add("acGridCrtHigh", 0);
                    obj.Add("acGridPwr", 0);
                    obj.Add("actPwrKw", pcsPacket.ActivePower);
                    obj.Add("rctPwrKw", pcsPacket.ReactivePower);
                    obj.Add("pwrFact", pcsPacket.PowerFactor);
                    obj.Add("acGridVltR", pcsPacket.AC_PhaseVoltage.R);
                    obj.Add("acGridVltS", pcsPacket.AC_PhaseVoltage.S);
                    obj.Add("acGridVltT", pcsPacket.AC_PhaseVoltage.T);
                    obj.Add("acGridCrtR", pcsPacket.AC_PhaseCurrent.R);
                    obj.Add("acGridCrtS", pcsPacket.AC_PhaseCurrent.S);
                    obj.Add("acGridCrtT", pcsPacket.AC_PhaseCurrent.T);
                    obj.Add("actCmdLimitLowChg", 0);
                    obj.Add("actCmdLimitLowDhg", 0);
                    obj.Add("actCmdLimitHighChg", batPacket.ChargePowerLimit);
                    obj.Add("actCmdLimitHighDhg", batPacket.DischargePowerLimit);
                    pcs_messages[pcsIdx] = CreateMessage(obj.ToString(), pcsPacket, 0, pcs_topic_name, timestamp);

                    obj = CreateTemporary(2, "PCS_BATTERY", $"BMS{pcsNo}", siteid, rccid, timestamp_string);
                    obj.Add("bms_soc", batPacket.SOC);
                    obj.Add("bms_soh", batPacket.SOH);
                    obj.Add("dcCellPwr", pcsPacket.DC_BatteryPower);
                    obj.Add("dcCellVlt", pcsPacket.DC_BatteryVoltage);
                    obj.Add("dcCellCrt", pcsPacket.DC_BatteryCurrent);
                    obj.Add("dcCellTmpMx", batPacket.ModuleTemp.Max);
                    obj.Add("dcCellTmpMn", batPacket.ModuleTemp.Min);
                    obj.Add("dcCellVltMx", batPacket.CellVoltage.Max);
                    obj.Add("dcCellVltMn", batPacket.CellVoltage.Min);
                    bat_messages[pcsIdx] = CreateMessage(obj.ToString(), batPacket,1, bat_topic_name, timestamp);

                    
                }

                JObject dc_obj = CreateTemporary(5, "DC", $"DC", siteid, rccid, timestamp_string);
                dc_obj.Add("KepcoTimer", dcPacket.KepcoTimer);
                dc_obj.Add("CurrentLoad", dcPacket.CurrentLoad);
                dc_obj.Add("ForecastingPower", dcPacket.ForecastingPower);
                dc_obj.Add("PreviousDemandPower", dcPacket.PreviousDemandPower);
                dc_obj.Add("AccumulatedPower", dcPacket.AccumulatedPower);
                dc_message = CreateMessage(dc_obj.ToString(), dcPacket, 5, dc_topic_name, timestamp);

                //obj = CreateTemporary(4, "PV_SYSTEM", $"PV{pv_pack.PmsIndex}", packet.sSiteId, timestamp);
                //obj.Add("TotalActivePower", pv_pack.TotalActivePower);
                //obj.Add("TotalReactivePower", pv_pack.TotalReactivePower);
                //obj.Add("ReverseActivePower", pv_pack.ReverseActivePower);
                //obj.Add("ReverseReactivePower", pv_pack.ReverseReactivePower);
                //obj.Add("vltR", pv_pack.Voltage.R);
                //obj.Add("vltS", pv_pack.Voltage.S);
                //obj.Add("vltT", pv_pack.Voltage.T);
                //obj.Add("crtR", pv_pack.Current.R);
                //obj.Add("crtS", pv_pack.Current.S);
                //obj.Add("crtT", pv_pack.Current.T);
                //obj.Add("Frequency", pv_pack.Frequency);
                //if (pv_pack.EnergyTotalActivePower != 0)
                //{

                //}
                //obj.Add("EnergyTotalActivePower", pv_pack.EnergyTotalActivePower);
                //obj.Add("EnergyTotalReactivePower", pv_pack.EnergyTotalReactivePower);
                //obj.Add("EnergyTotalReverseActivePower", pv_pack.EnergyTotalReverseActivePower);
                //pv_string = obj.ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static JObject CreateTemporary(int groupId, string groupName, string deviceId, int siteId, int rcc, string timestamp)
        {
            JObject newObj = new JObject();
            newObj.Add("groupid", groupId);
            newObj.Add("groupname", groupName);
            newObj.Add("deviceId", deviceId);
            newObj.Add("normalizedeviceid", deviceId);
            newObj.Add("siteId", siteId);
            newObj.Add("rcc", rcc);
            newObj.Add("inJeju", 0);
            newObj.Add("timestamp", timestamp);
            return newObj;

        }
    }
}
