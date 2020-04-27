using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public class PCSMeasurement : MeasurementValue
    {
        /// <summary>
        /// DC 소스 전압
        /// </summary>
        public float DCSrcVlt { get; set; }
        /// <summary>
        /// DC 소스 전류
        /// </summary>
        public float DCSrcCrt { get; set; }
        /// <summary>
        /// DC 소스 전력
        /// </summary>
        public float DCSrcPwr { get; set; }

        /// <summary>
        /// PCS 3상 전압
        /// </summary>
        public RST PCSVlt { get; set; } = RST.Empty;

        /// <summary>
        /// PCS 3상 전류
        /// </summary>
        public RST PCSCrt { get; set; } = RST.Empty;

        /// <summary>
        /// Grid 3상 전압
        /// </summary>
        public RST GridVlt { get; set; } = RST.Empty;

        /// <summary>
        /// Grid 3상 전류
        /// </summary>
        public RST GridCrt { get; set; } = RST.Empty;

        /// <summary>
        /// AC 충전 누적 전력
        /// </summary>
        public float ACChgAccPwr { get; set; }
        /// <summary>
        /// AC 방전 누적 전력
        /// </summary>
        public float ACDhgAccPwr { get; set; }
        /// <summary>
        /// AC 유효전력
        /// </summary>
        public float ACActPwr { get; set; }
        /// <summary>
        /// SOC 값
        /// </summary>
        public float SOC { get; set; }
        public float SOH { get; set; }

        /// <summary>
        /// PCS 유효/무효 전력
        /// </summary>
        public PairPower PCSPwr { get; set; } = PairPower.Empty;

        /// <summary>
        /// PCS 유효/무효 전력 명령어 레퍼런스 값
        /// </summary>
        public PairPower RefPCSPwr { get; set; } = PairPower.Empty;


        /// <summary>
        /// 입력 유효 전력량 (충전)
        /// </summary>
        public float InputAvlPwr { get; set; }
        /// <summary>
        /// 출력 유효전력량 (방전)
        /// </summary>
        public float outputAvlPwr { get; set; }

        /// <summary>
        /// 역률
        /// </summary>
        public double PwrFactor { get; set; }

        /// <summary>
        /// 주파수
        /// </summary>
        public double Frequency { get; set; }


        public PCSMeasurement()
        {

        }

        ~PCSMeasurement()
        {

        }
    }
}
