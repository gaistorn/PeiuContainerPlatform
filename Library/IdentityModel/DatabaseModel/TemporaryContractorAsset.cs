using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [Table("TemporaryContractorAssets")]
    public class TemporaryContractorAssetEF
    {
        [Key]
        public string UniqueId { get; set; }

        [ForeignKey("ContractorSite")]
        public string SiteId { get; set; }

        public TemporaryContractorSiteEF ContractorSite { get; set; }

        public AssetTypeCodes AssetType { get; set; }

        public double CapacityKW { get; set; }

        public string AssetName { get; set; }

        public TemporaryContractorAssetEF() { }
    }
}
