using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public class BSCSensorValue : SensorGroupValue
    {
        /// <summary>
        /// 셀 온도 최대/최소 범위
        /// </summary>
        public FloatingRange TempOfCellRange { get; set; }

        public override SensorValue[] Sensors
        {
            get => new SensorValue[]
            {
                new SensorValue() {Name = "MinTempOfCellRange", Value = TempOfCellRange.Min },
                new SensorValue() {Name = "MaxTempOfCellRange", Value = TempOfCellRange.Max }
            };
            set => base.Sensors = value;
        }
    }
}
