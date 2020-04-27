using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PeiuPlatform.Model.Database
{


    [Table("UserLogs")]
    public class UserLog : UserLogBase
    {
        [Key]
        public override int UniqueId { get; set; }
    }

    public class UserLogBase
    {
        public virtual int UniqueId { get; set; }
        public DateTime CreateDT { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
        public int Level { get; set; }

        public string From { get; set; }
    }
}
