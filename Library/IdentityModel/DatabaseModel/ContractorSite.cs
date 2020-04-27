using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [Table("ContractorSites")]
    public class ContractorSiteEF : ContractorSiteBase
    {
        public ContractorSiteEF() { }

        [Key]
        public override int SiteId { get; set; }

        [ForeignKey("ContractUser")]
        public  override string ContractUserId { get; set; }

        public virtual ContractorUserEF ContractUser { get; set; }

        public virtual ICollection<ContractorAssetEF> ContractorAssets { get; set; }

    }

    public class ContractorSiteBase
    {
        public ContractorSiteBase() { }

        public virtual int SiteId { get; set; }
        public virtual int RCC { get; set; }
        public virtual double Longtidue { get; set; }
        public virtual double Latitude { get; set; }
        public virtual string LawFirstCode { get; set; }
        public virtual string LawMiddleCode { get; set; }
        public virtual string LawLastCode { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string Represenation { get; set; } = "미설정";
        public virtual int? DLNo { get; set; }
        public virtual string ContractUserId { get; set; }

        public virtual bool RestrictSite { get; set; }

        public virtual int DeviceGroupCode { get; set; }

        public virtual ServiceCodes ServiceCode { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime RegisterTimestamp { get; set; }

        public override string ToString()
        {
            return $"SiteId {SiteId} / {Address1}";
        }

    }
}
