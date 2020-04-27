using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [Table("AggregatorUsers")]
    public class AggregatorUserEF : AggregatorUserBase
    {
        [Key]
        public override string UserId { get; set; }

        [ForeignKey("AggregatorGroup")]
        public override string AggGroupId { get; set; }


        [Newtonsoft.Json.JsonIgnore]
        public virtual AggregatorGroupEF AggregatorGroup { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual UserAccountEF User { get; set; }

        public AggregatorUserEF() { }
    }

    public class AggregatorUserBase
    {
        public virtual string UserId { get; set; }

        [ForeignKey("AggregatorGroup")]
        public virtual string AggGroupId { get; set; }

        public AggregatorUserBase() { }
    }
}
