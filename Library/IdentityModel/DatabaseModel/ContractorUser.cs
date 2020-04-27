using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("ContractorUsers")]
    public class ContractorUserEF
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string UserId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("AggregatorGroup")]
        public string AggGroupId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual AggregatorGroupEF AggregatorGroup { get; set; }

        ////[System.ComponentModel.DataAnnotations.Schema.ForeignKey("User")]
        //public string UserId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual UserAccountEF User { get; set; }

        public virtual ICollection<ContractorSiteEF> ContractorSite { get; set; } = new HashSet<ContractorSiteEF>();


        public virtual ContractStatusCodes ContractStatus { get; set; } = ContractStatusCodes.Signing;

        

        public ContractorUserEF() { }
    }
}
