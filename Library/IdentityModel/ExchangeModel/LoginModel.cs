using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PeiuPlatform.Model.ExchangeModel
{
    public class LoginModel : LoginModelView
    {
        [Required]
        [EmailAddress]
        public override string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public override string Password { get; set; }

        [Display(Description = "Remember me?")]
        public override bool RememberMe { get; set; }
    }

    public class LoginModelView
    {
        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual bool RememberMe { get; set; }
    }
}
