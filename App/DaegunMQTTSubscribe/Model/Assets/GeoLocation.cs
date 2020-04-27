using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Models
{
    public class GeoLocation
    {
        /// <summary>
        /// 위도
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// 경도
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// 법정동 코드 Major
        /// </summary>
        public ushort MajorLawCode { get; set; }

        /// <summary>
        /// 법정동 코드 Mid
        /// </summary>
        public ushort MidLawCode { get; set; }

        /// <summary>
        /// 법정동 코드 Minor
        /// </summary>
        public ushort MinorLawCode { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string LawCode => string.Format("{0}-{1}-{2}", MajorLawCode, MidLawCode, MinorLawCode);

        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }

    }
}
