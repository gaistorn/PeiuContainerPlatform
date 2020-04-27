using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    public class UserClaim : IdentityUserClaim<string>
    {
        public UserClaim() { }
    }
}
