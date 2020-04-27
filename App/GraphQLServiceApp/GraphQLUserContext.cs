using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;
using PeiuPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class GraphQLUserContext : Dictionary<string, object>
    {
        public const string SiteIdsByRccClaim = "SiteIdsByRcc";
        public ClaimsPrincipal UserClaim { get; set; }

        private HashSet<int> _allSiteIds = new HashSet<int>();
        public HashSet<int> AllSiteIds
        {
            get
            {
                return _allSiteIds;
            }
        }

        private Dictionary<int, List<int>> _siteIds;

        public Dictionary<int, List<int>> SiteIds => _siteIds ?? (_siteIds = GetSiteIdsClaimValue());

        internal void UpdateClaimsValue()
        {
            if(_siteIds == null)
            _siteIds = GetSiteIdsClaimValue();
        }

        private  Dictionary<int, List<int>> GetSiteIdsClaimValue()
        {
            
            var claim = UserClaim.FindFirst(SiteIdsByRccClaim);
            string siteidsValue = claim.Value;
            string[] splits = siteidsValue.Split(',');
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            _allSiteIds.Clear();
            foreach (string word in splits)
            {
                string[] words = word.Split(':');
                int rcc = int.Parse(words[0]);
                int siteid = int.Parse(words[1]);
                if (result.ContainsKey(rcc) == false)
                    result.Add(rcc, new List<int>());
                if (_allSiteIds.Contains(siteid) == false)
                    _allSiteIds.Add(siteid);
                result[rcc].Add(siteid);
            }
            return result;
        }
    }
}
