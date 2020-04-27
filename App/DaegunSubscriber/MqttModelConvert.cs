using Newtonsoft.Json.Linq;
using PeiuPlatform.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public static class MqttModelConvert
    {
        static MqttModelConvert()
        {
            LoadRccCodeFile();
        }

        private static void LoadRccCodeFile()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rcccode.txt");
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();
                    string[] words = line.Split('\t');
                    int rcc = int.Parse(words[1]);
                    int siteid = int.Parse(words[0]);

                    if (RccCodeList.ContainsKey(siteid) == false)
                    {
                        RccCodeList.Add(siteid, rcc);
                    }
                    else
                        RccCodeList[siteid] = rcc;

                }
            }
        }

        public static Dictionary<int, int> RccCodeList = new Dictionary<int, int>();
        public static bool ConvertPcs(DaegunPacket packet, DateTime timestamp, ref string pcs_string, ref string bms_string, ref string pv_string)
        {
            
            DaegunPcsPacket pcsPacket = packet.Pcs;
            DaegunBSCPacket bmsPacket = packet.Bsc;
            DaegunMeterPacket bms_meter_packet = packet.Ess;
            DaegunMeterPacket pv_pack = packet.Pv;
            JObject obj = CreateTemporary(1, "PCS_SYSTEM", $"PCS{pcsPacket.PcsNumber}", packet.sSiteId, timestamp);
            obj.Add("freq", pcsPacket.Frequency);
            obj.Add("acGridVlt", 0);
            obj.Add("acGridCrtLow", 0);
            obj.Add("acGridCrtHigh", 0);
            obj.Add("acGridPwr", 0);
            obj.Add("actPwrKw", pcsPacket.ActivePower);
            obj.Add("rctPwrKw", pcsPacket.ReactivePower);
            obj.Add("pwrFact", pcsPacket.PowerFactor);
            obj.Add("acGridVltR", pcsPacket.AC_phase_voltage.R);
            obj.Add("acGridVltS", pcsPacket.AC_phase_voltage.S);
            obj.Add("acGridVltT", pcsPacket.AC_phase_voltage.T);
            obj.Add("acGridCrtR", pcsPacket.AC_phase_current.R);
            obj.Add("acGridCrtS", pcsPacket.AC_phase_current.S);
            obj.Add("acGridCrtT", pcsPacket.AC_phase_current.T);
            obj.Add("actCmdLimitLowChg", 0);
            obj.Add("actCmdLimitLowDhg", 0);
            obj.Add("actCmdLimitHighChg", bmsPacket.ChargePowerLimit);
            obj.Add("actCmdLimitHighDhg", bmsPacket.DischargePowerLimit);
            pcs_string = obj.ToString();

            obj = CreateTemporary(2, "PCS_BATTERY", $"BMS{bmsPacket.PcsIndex}", packet.sSiteId, timestamp);
            obj.Add("bms_soc", bmsPacket.Soc);
            obj.Add("bms_soh", bmsPacket.Soh);
            obj.Add("dcCellPwr", packet.Pcs.Dc_battery_power);
            obj.Add("dcCellVlt", packet.Pcs.Dc_battery_voltage);
            obj.Add("dcCellCrt", packet.Pcs.Dc_battery_current);
            obj.Add("dcCellTmpMx", packet.Bsc.ModuleTempRange.Max);
            obj.Add("dcCellTmpMn", packet.Bsc.ModuleTempRange.Min);
            obj.Add("dcCellVltMx", packet.Bsc.CellVoltageRange.Max);
            obj.Add("dcCellVltMn", packet.Bsc.CellVoltageRange.Min);
            bms_string = obj.ToString();

            obj = CreateTemporary(4, "PV_SYSTEM", $"PV{pv_pack.PmsIndex}", packet.sSiteId, timestamp);
            obj.Add("TotalActivePower", pv_pack.TotalActivePower);
            obj.Add("TotalReactivePower", pv_pack.TotalReactivePower);
            obj.Add("ReverseActivePower", pv_pack.ReverseActivePower);
            obj.Add("ReverseReactivePower", pv_pack.ReverseReactivePower);
            obj.Add("vltR", pv_pack.Voltage.R);
            obj.Add("vltS", pv_pack.Voltage.S);
            obj.Add("vltT", pv_pack.Voltage.T);
            obj.Add("crtR", pv_pack.Current.R);
            obj.Add("crtS", pv_pack.Current.S);
            obj.Add("crtT", pv_pack.Current.T);
            obj.Add("Frequency", pv_pack.Frequency);
            if(pv_pack.EnergyTotalActivePower != 0)
            {

            }
            obj.Add("EnergyTotalActivePower", pv_pack.EnergyTotalActivePower);
            obj.Add("EnergyTotalReactivePower", pv_pack.EnergyTotalReactivePower);
            obj.Add("EnergyTotalReverseActivePower", pv_pack.EnergyTotalReverseActivePower);
            pv_string = obj.ToString();
            return true;
        }

        private static JObject CreateTemporary(int groupId, string groupName, string deviceId, int siteId, DateTime timestamp)
        {
            JObject newObj = new JObject();
            newObj.Add("groupid", groupId);
            newObj.Add("groupname", groupName);
            newObj.Add("deviceId", deviceId);
            newObj.Add("normalizedeviceid", deviceId);
            newObj.Add("siteId", siteId);
            if(RccCodeList.ContainsKey(siteId))
                newObj.Add("rcc", RccCodeList[siteId]);
            else
                newObj.Add("rcc", -1);
            newObj.Add("inJeju", 0);
            newObj.Add("timestamp", timestamp.ToString("yyyyMMddHHmmss"));

            return newObj;

        }
    }
}
