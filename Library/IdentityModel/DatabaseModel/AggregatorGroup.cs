using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [Table("AggregatorGroups")]
    public class AggregatorGroupEF : AggregatorGroupBase
    {
        [Key]
        public override string ID { get; set; }

        public virtual ICollection<AggregatorUserEF> AggregatorUsers { get; set; } = new HashSet<AggregatorUserEF>();

        public virtual ICollection<ContractorUserEF> ContractorUsers { get; set; } = new HashSet<ContractorUserEF>();

        public AggregatorGroupEF() { }
    }

    public class AggregatorGroupBase
    {
        public virtual string ID { get; set; }

        /// <summary>
        /// 어그리게이트 명
        /// </summary>
        public virtual string AggName { get; set; }

        /// <summary>
        /// 대표 상호명
        /// </summary>
        public virtual string Representation { get; set; }

        public virtual string Address { get; set; }

        public virtual DateTime CreateDT { get; set; }

        public virtual string PhoneNumber { get; set; }

        

        public AggregatorGroupBase() { }
    }
}
