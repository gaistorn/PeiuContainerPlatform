using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public class BSCMeasurementValue : MeasurementValue
    {
        public float SOC { get; set; }
        public float SOH { get; set; }
        /// <summary>
		/// DC 전압
		/// </summary>
		public float DCVlt{ get; set; }
        /// <summary>
        /// DC 전력
        /// </summary>
        public float DCPwr{ get; set; }
        /// <summary>
        /// DC 조류
        /// </summary>
        public float DCCrt{ get; set; }
        /// <summary>
        /// DC 충전 누적 전력량
        /// </summary>
        public double DCChgAccPwr{ get; set; }
        /// <summary>
        /// DC 방전 누적 전력량
        /// </summary>
        public double DCDhgAccPwr{ get; set; }

        /// <summary>
        /// Cell 최소/최대 전압 범위
        /// </summary>
        public FloatingRange VltOfCellRange { get; set; } = FloatingRange.Empty;

        /// <summary>
        /// Sum of all the cell voltage
        /// </summary>
        public double SumOfAllCellVlt{ get; set; }
        /// <summary>
        /// Bank DC Charge Current Limit
        /// </summary>
        public float BkDCChgCrtlmt{ get; set; }
        /// <summary>
        /// Bank DC Discharge Current Limit
        /// </summary>
        public float BkDCDhgCrtlmt{ get; set; }
        /// <summary>
        /// Bank DC Discharge Power limit
        /// </summary>
        public float BkDCDhgPwrlmt{ get; set; }
        /// <summary>
        /// Bank Average Cell Voltage
        /// </summary>
        public float BkAvgCellVlt{ get; set; }
        /// <summary>
        /// Bank Highest Cell Voltage
        /// </summary>
        public float BkHiCellVlt{ get; set; }
        /// <summary>
        /// Bank Lowest Cell Voltage
        /// </summary>
        public float BkLoCellVlt{ get; set; }

        /// <summary>
        /// Bank Lowest Cell Voltage location
        /// </summary>
        public float BkLoCellVltLoc{ get; set; }

        /// <summary>
        /// Bank Highest Cell Voltage location
        /// </summary>
        public float BkHiCellVltLoc { get; set; }
    }
}
