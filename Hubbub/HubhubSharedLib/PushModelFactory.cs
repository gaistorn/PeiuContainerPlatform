using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Hubbub
{
    public static class PushModelFactory
    {
        //private readonly static string[] PCS_ELEMENTS = new[]
        //{
        //    "freq",
        //    "acGridVlt",
        //    "acGridVlt",
        //    "acGridCrtLow",
        //    "acGridCrtHigh",
        //    "acGridPwr",
        //    "actPwrKw",
        //    "rctPwrKw",
        //    "pwrFact",
        //    "acGridVltR",
        //    "acGridVltS",
        //    "acGridVltT",
        //    "acGridCrtR",
        //    "acGridCrtS",
        //    "acGridCrtT",
        //    "actCmdLimitLowChg",
        //    "actCmdLimitLowDhg",
        //    "actCmdLimitHighChg",
        //    "actCmdLimitHighDhg"
        //};
        //private readonly static string[] BMS_ELEMENTS = new[]
        //{
        //    "bms_soc",
        //    "bms_soh",
        //    "dcCellPwr",
        //    "dcCellVlt",
        //    "dcCellCrt",
        //    "dcCellTmpMx",
        //    "dcCellTmpMn",
        //    "dcCellVltMx",
        //    "dcCellVltMn",
        //};
        //private readonly static string[] PV_ELEMENTS = new[]
        //{
        //    "TotalActivePower",
        //    "TotalReactivePower",
        //    "ReverseActivePower",
        //    "ReverseReactivePower",
        //    "vltR",
        //    "vltS",
        //    "vltT",
        //    "crtR",
        //    "crtS",
        //    "crtT",
        //    "Frequency"
        //};

        public static JObject CreateDIModel(int DeviceIndex, int SiteId, int rcc, DeviceTypes deviceType,
            IDictionary<int, IComparable> Values,
            IDictionary<int, VwModbusInputPoint> Points
            )
        {
            int groupId = -1;
            string groupname = null;

            //ReadGroupId(deviceType, out groupId, out groupname);
            JObject payload = CreateBaseModel(deviceType, DeviceIndex, SiteId, rcc);
            foreach (int ai_id in Values.Keys)
            {
                IComparable value = Values[ai_id];
                VwModbusInputPoint point = Points[ai_id];
                payload.Add(point.Name, value.ToString());
            }
            return payload;
        }

        public static JObject CreateDigitalModel(DeviceTypes deviceTypes, int deviceindex, int siteid, int rcc)
        {
            JObject @digitalModel = CreateDefaultModel(deviceTypes, deviceindex, siteid, rcc);
            digitalModel.Add("ditype", 0);
            digitalModel.Add("devicetype", (int)deviceTypes);
            digitalModel.Add("deviceindex", deviceindex);
            return digitalModel;
        }

        public static JObject CreatePcsStatusModel(int deviceIndex, int siteid, int rcc, 
            IEnumerable<VwStandardPcsStatusPoint> pcsStatusPoints,
            IDictionary<int, ModbusDigitalStatusPoint> modbusDigitalStatuses,
            IDictionary<int, ushort> values)
        {
            JObject model = CreateDefaultModel(11, "PCS_STATUS", DeviceTypes.PCS, deviceIndex, siteid, rcc);
            ValidationStatusModel(model, pcsStatusPoints, modbusDigitalStatuses, values);
            return model;
            //ValidationStatusModel
        }

        public static JObject CreateAnalogModel(DeviceTypes devicetype, int deviceindex, int siteid, int rcc, IEnumerable<VwStandardAnalogPoint> standardAnalogs)
        {
            JObject @analogModel = CreateDefaultModel(devicetype, deviceindex, siteid, rcc);
            ValidationAnalogModel(@analogModel, standardAnalogs.Where(x => x.Deviceid == (int)devicetype));
            return @analogModel;
        }

        public static JObject CreateDefaultModel(int groupid, string groupname, DeviceTypes devicetype, int deviceindex, int siteid, int rcc)
        {
            JObject model = new JObject();
            model.Add("groupid", groupid);
            model.Add("groupname", groupname);
            string name = $"{devicetype}{deviceindex }";
            model.Add("deviceid", name);
            model.Add("normalizedeviceid", name);
            model.Add("siteId", siteid);
            model.Add("rcc", rcc);
            model.Add("isJueju", rcc == 16 ? 1 : 0);
            string timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            model.Add("timestamp", timestamp);
            string utctimestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            model.Add("utctimestamp", utctimestamp);
            return model;
        }

        public static JObject CreateDefaultModel(DeviceTypes devicetype, int deviceindex, int siteid, int rcc)
        {
            JObject model = new JObject();
            int groupid = 1;
            string groupname = null;
            ReadGroupId(devicetype, out groupid, out groupname);
            model.Add("groupid", groupid);
            model.Add("groupname", groupname);
            string name = $"{devicetype}{deviceindex }";
            model.Add("deviceid", name);
            model.Add("normalizedeviceid", name);
            model.Add("siteId", siteid);
            model.Add("rcc", rcc);
            model.Add("isJueju", rcc == 16 ? 1 : 0);
            string timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            model.Add("timestamp", timestamp);
            string utctimestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            model.Add("utctimestamp", utctimestamp);
            return model;
        }

       

        public static JObject CreateAIModel(int DeviceIndex, int SiteId, int rcc, DeviceTypes deviceType, 
            IDictionary<int, IComparable> Values,
            IEnumerable<VwStandardAnalogPoint> standardAnalogs,
            IDictionary<int, VwModbusInputPoint> Points
            )
        {
            int groupId = -1;
            string groupname = null;

            ReadGroupId(deviceType, out groupId, out groupname);
            JObject payload = CreateBaseModel(deviceType, DeviceIndex, SiteId, rcc);
            foreach (int ai_id in Values.Keys)
            {
                IComparable value = Values[ai_id];
                VwModbusInputPoint point = Points[ai_id];
                payload.Add(point.Name, value.ToString());
            }

            ValidationAnalogModel(payload, standardAnalogs.Where(x => x.Deviceid == (int)deviceType));
            return payload;
            //payload.Add("freq", pcsPacket.Frequency);
            //payload.Add("acGridVlt", 0);
            //payload.Add("acGridCrtLow", 0);
            //payload.Add("acGridCrtHigh", 0);
            //payload.Add("acGridPwr", 0);
            //payload.Add("actPwrKw", pcsPacket.ActivePower);
            //payload.Add("rctPwrKw", pcsPacket.ReactivePower);
            //payload.Add("pwrFact", pcsPacket.PowerFactor);
            //payload.Add("acGridVltR", pcsPacket.AC_phase_voltage.R);
            //payload.Add("acGridVltS", pcsPacket.AC_phase_voltage.S);
            //payload.Add("acGridVltT", pcsPacket.AC_phase_voltage.T);
            //payload.Add("acGridCrtR", pcsPacket.AC_phase_current.R);
            //payload.Add("acGridCrtS", pcsPacket.AC_phase_current.S);
            //payload.Add("acGridCrtT", pcsPacket.AC_phase_current.T);
            //payload.Add("actCmdLimitLowChg", 0);
            //payload.Add("actCmdLimitLowDhg", 0);
            //payload.Add("actCmdLimitHighChg", bmsPacket.ChargePowerLimit);
            //payload.Add("actCmdLimitHighDhg", bmsPacket.DischargePowerLimit);
            //pcs_string = payload.ToString();

            //obj = CreateTemporary(2, "PCS_BATTERY", $"BMS{bmsPacket.PcsIndex}", packet.sSiteId, timestamp);
            //obj.Add("bms_soc", bmsPacket.Soc);
            //obj.Add("bms_soh", bmsPacket.Soh);
            //obj.Add("dcCellPwr", packet.Pcs.Dc_battery_power);
            //obj.Add("dcCellVlt", packet.Pcs.Dc_battery_voltage);
            //obj.Add("dcCellCrt", packet.Pcs.Dc_battery_current);
            //obj.Add("dcCellTmpMx", packet.Bsc.ModuleTempRange.Max);
            //obj.Add("dcCellTmpMn", packet.Bsc.ModuleTempRange.Min);
            //obj.Add("dcCellVltMx", packet.Bsc.CellVoltageRange.Max);
            //obj.Add("dcCellVltMn", packet.Bsc.CellVoltageRange.Min);
            //bms_string = obj.ToString();

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
            //return true;
        }

        private static void ValidationStatusModel(JObject @object, 
            IEnumerable<VwStandardPcsStatusPoint> standardPcsStatusPoints,
            IDictionary<int, ModbusDigitalStatusPoint> modbusDigitalStatuses,
            IDictionary<int, ushort> values)
        {
            foreach (VwStandardPcsStatusPoint analogPoint in standardPcsStatusPoints)
            {
                int st = 0;
                if(modbusDigitalStatuses.ContainsKey(analogPoint.Id))
                {
                    var stPoint = modbusDigitalStatuses[analogPoint.Id];
                    if(values.ContainsKey(stPoint.GetGroupCode()))
                    {
                        ushort value = values[stPoint.GetGroupCode()];
                        bool IsMatch = stPoint.Match == 1;
                        st = ((value & stPoint.Bitflag) == stPoint.Bitflag) == IsMatch ? 1 : 0;
                    }
                }

                if (@object.ContainsKey(analogPoint.Name) == false)
                    @object.Add(analogPoint.Name, st);
            }
        }

        private static void ValidationAnalogModel(JObject @object, IEnumerable<VwStandardAnalogPoint> standardAnalogPoints)
        {
            foreach(VwStandardAnalogPoint analogPoint in standardAnalogPoints)
            {
                if(@object.ContainsKey(analogPoint.Fieldname) == false)
                    @object.Add(analogPoint.Fieldname, 0);
            }
        }

        private static void ReadGroupId(DeviceTypes deviceType, out int GroupId, out string GroupName)
        {
            switch(deviceType)
            {
                case DeviceTypes.PCS:
                    GroupId = 1;
                    GroupName = "PCS_SYSTEM";
                    break;
                case DeviceTypes.BMS:
                    GroupId = 2;
                    GroupName = "PCS_BATTERY";
                    break;
                case DeviceTypes.PV:
                    GroupId = 4;
                    GroupName = "PV_SYSTEM";
                    break;
                case DeviceTypes.PCS_STATUS:
                    GroupId = 5;
                    GroupName = "PCS_STATUS";
                    break;
                default:
                    GroupId = -1;
                    GroupName = "UNKNOWN_SYSTEM";
                    break;
            }
        }

        private static JObject CreateBaseModel(DeviceTypes DeviceType, int DeviceIndex, int siteId, int rcc)
        {
            int groupId;
            string groupName;
            ReadGroupId(DeviceType, out groupId, out groupName);
            JObject newObj = new JObject();
            newObj.Add("groupid", groupId);
            newObj.Add("groupname", groupName);
            string id = $"{DeviceType}{DeviceIndex}";
            newObj.Add("deviceId", id);
            newObj.Add("normalizedeviceid", id);
            newObj.Add("siteId", siteId);
            newObj.Add("rcc", rcc);
            newObj.Add("inJeju", rcc == 16 ? 1 : 0);
            string timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            newObj.Add("timestamp", timestamp);
            string utctimestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
            newObj.Add("utctimestamp", utctimestamp);
            return newObj;
        }
    }
}
