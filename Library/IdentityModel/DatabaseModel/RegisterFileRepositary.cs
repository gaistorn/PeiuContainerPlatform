using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("RegisterFileRepositary")]
    public class RegisterFileRepositaryEF
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string UserId { get; set; }

        public string FileName { get; set; }

        public byte[] Contents { get; set; }

        public DateTime CreateDT { get; set; }

        public string ContentsType { get; set; }
    }
}
