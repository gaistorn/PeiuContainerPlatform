using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "password")]
        public string NewPassword { get; set; }

        [Required]
        //[DataType(DataType.to)]
        [Display(Name = "token")]
        public string Token { get; set; }
    }
}
