using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public class PVMeasurement : MeasurementValue
    {
        /// <summary>
		/// 시스템 상태
		/// </summary>
		public int Systemstatus{ get; set; }
        /// <summary>
        /// 금일 동작한 시간
        /// </summary>
        public int ActivationTimeToday{ get; set; }
        /// <summary>
        /// 금일 발전 시작 시간
        /// </summary>
        public int GenerationStartTimeToday{ get; set; }
        /// <summary>
        /// 금일 발전 정지 시간
        /// </summary>
        public int GenerationEndTimeToday{ get; set; }
        /// <summary>
        /// 총 동작 시간
        /// </summary>
        public int ActivationAllTime{ get; set; }
        /// <summary>
        /// 누적 발전량
        /// </summary>
        public double AccumTotalGeneration{ get; set; }
        /// <summary>
        /// 10분 발전량
        /// </summary>
        public double GenerationOnTenMin{ get; set; }
        /// <summary>
        /// 금시 발전량
        /// </summary>
        public double GenerationOnHour{ get; set; }
        /// <summary>
        /// 금일 발전량
        /// </summary>
        public double GenerationOnDay{ get; set; }
        /// <summary>
        /// 금월 발전량
        /// </summary>
        public double GenerationOnMonth{ get; set; }
        /// <summary>
        /// 금년 발전량
        /// </summary>
        public double GenerationOnYear{ get; set; }
        /// <summary>
        /// 계통 최대 전압
        /// </summary>
        public float MaxGridVlt{ get; set; }
        /// <summary>
        /// 계통 최저전압
        /// </summary>
        public float MinGridVlt{ get; set; }
        /// <summary>
        /// 계통최대 주파수
        /// </summary>
        public float MaxGridFreq{ get; set; }
        /// <summary>
        /// 계통 최저주파수
        /// </summary>
        public float MinGridFreq{ get; set; }
        /// <summary>
        /// MPP 최저 전압
        /// </summary>
        public float MinMPPVlt{ get; set; }
        /// <summary>
        /// 발전 대기시간
        /// </summary>
        public int GenerationTimeOnDelay{ get; set; }
        /// <summary>
        /// 저출력 유효시간
        /// </summary>
        public float LowOutputAvaliableTime{ get; set; }
        /// <summary>
        /// PV 동작 전압
        /// </summary>
        public float EngageVlt{ get; set; }
        /// <summary>
        /// PV 최저 전압
        /// </summary>
        public float MinVoltage{ get; set; }

        /// <summary>
		/// PV 전압
		/// </summary>
		public float PvVlt{ get; set; }
        /// <summary>
        /// DC 링크 전압
        /// </summary>
        public float DCLinkVlt{ get; set; }
        /// <summary>
        /// PV 전류
        /// </summary>
        public float PvCrt{ get; set; }
        /// <summary>
        /// PV 전력
        /// </summary>
        public float PvPwr{ get; set; }
        /// <summary>
        /// 인버터 출력 전압
        /// </summary>
        public float IvtOutVlt{ get; set; }
        /// <summary>
        /// 인버터 출력 전류
        /// </summary>
        public float IvtOutCrt{ get; set; }
        /// <summary>
        /// 인버터 출력 전력
        /// </summary>
        public float IvtOutPwr{ get; set; }

        /// <summary>
        /// 인버터 동작 상태
        /// </summary>
        public int IvtOperStatus { get; set; }
        /// <summary>
        /// 인버터 상태
        /// </summary>
        public int IvtStatus { get; set; }
    }
}
