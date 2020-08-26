using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    public class PushModel 
    {
        public string Topic { get; private set; }
        public JObject Template { get; private set; }

        public static PushModel CreateAnalogInputPushModel(JObject payload, int siteid, int deviceid, int deviceindex)
        {
            PushModel model = new PushModel(payload);
            model.Topic = $"{HubbubInformation.GlobalHubbubInformation.topicroot}/{siteid}/{GetDeviceName(deviceid)}{deviceindex}/AI";
            return model;
        }

        private static string GetDeviceName(int deviceid)
        {
            DeviceTypes type = (DeviceTypes)deviceid;
            return Enum.GetName(typeof(DeviceTypes), type);
        }

        public static PushModel CreateDigitalStatusModel(JObject payload, int siteid, int deviceid, int deviceindex)
        {
            PushModel model = new PushModel(payload);
            model.Topic = $"{HubbubInformation.GlobalHubbubInformation.topicroot}/{siteid}/{deviceid}/{deviceindex}/stat";
            //hubbub / 6 / 0 / 1 / stat
            return model;
        }

        public static PushModel CreateDigitalInputPushModel(JObject payload, int siteid, int deviceid, int deviceindex)
        {
            PushModel model = new PushModel(payload);
            model.Topic = $"{HubbubInformation.GlobalHubbubInformation.topicroot}/{siteid}/{deviceid}/{deviceindex}/di";
            return model;

            
        }

        private PushModel(JObject payload)
        {
            this.Template = payload;
        }
    }

    
}
