using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.IdentityModel
{
    public class UserPolicyTypes
    {
        public const string AllUserPolicy = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/alluserpolicy";
        public const string RequiredManager = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/requiredmanager";
        public const string OnlySupervisor = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/onlysupervisor";
        public const string HubbubManager = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/hubbubmanager";
        public const string AccountManager = CommonTypes.IDENTITY_NAMESPACE_URI + "/policy/accountmanager";
    }
}
