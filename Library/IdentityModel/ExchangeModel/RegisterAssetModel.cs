using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class RegisterAssetModel
    {
        public AssetTypeCodes Type { get; set; }
        public double CapacityMW { get; set; }
        
        public int Index { get; set; }
    }
}
