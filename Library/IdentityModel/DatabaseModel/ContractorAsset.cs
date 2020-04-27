using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [Table("ContractorAssets")]
    public class ContractorAssetEF
    {
        [Key]
        public int UniqueId { get; set; }

        [ForeignKey("ContractorSite")]
        public int SiteId { get; set; }

        [JsonIgnore]
        public ContractorSiteEF ContractorSite { get; set; }

        public AssetTypeCodes AssetType { get; set; }

        public double CapacityKW { get; set; }

        public DateTime InstallDate { get; set; }

        public short DeviceIndex { get; set; }
        public DateTime LastMaintenance { get; set; }

        public string DeviceId { get; set; }

        public ContractorAssetEF() { }
    }
}
