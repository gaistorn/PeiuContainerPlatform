using FireworksFramework.Mqtt;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App.Publisher
{
    public class ReservedRegisterNotifyPublisher : AbsMqttPublisher
    {
        public ReservedRegisterNotifyPublisher() : base()
        {
            
        }

        protected override string GetMqttPublishTopicName()
        {
            return "hubbub/notification/reservedassetlocation";
        }
    }
}
