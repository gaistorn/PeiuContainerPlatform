using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [Table("SupervisorUsers")]
    public class SupervisorUserEF
    {
        [Key]
        public string UserId { get; set; }

        public virtual UserAccountEF User { get; set; }

        public SupervisorUserEF() { }
    }
}
