using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public abstract class ValueTemplate
    {
        public abstract ValuesCategory Category {get;}
        public string DeviceId { get; set; }
        public DateTime MeasureTime { get; set; }
        public DateTime RecordTime { get; set; }
    }

    public enum DeviceType
    {
        PCS,
        BSC,
        PV_Meter,
        ESS_Meter
    }

    public enum ValuesCategory
    {
        Measurement,
        Sensor,
        Sample,
        Status
    }

    public enum StatusCategory
    {
        Fault,
        Warning,
        Status
    }
}
