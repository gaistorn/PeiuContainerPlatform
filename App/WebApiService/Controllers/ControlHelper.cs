using Microsoft.AspNetCore.Http;
using PeiuPlatform.App;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PeiuPlatform.Model.Database;
using PeiuPlatform.Model.IdentityModel;

namespace WebApiService.Controllers
{
    public static  class ControlHelper
    {
        public static string GetSiteIdsClaim(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext)
        {
            var result = GetAvaliableRccCodes(_accountContext, _claimsManager, httpContext);
            List<string> codes = new List<string>();
            foreach (IGrouping<int, int> row in result)
            {
                codes.Add($"{row.Key}:{row}");
            }
            return string.Join(',', codes);
        }

       

        //public static Dictionary<int, List<int>> GetSiteIdsClaimValue(IClaimServiceFactory _claimsManager, HttpContext httpContext)
        //{
        //    string siteidsValue = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.SiteIdsByRcc);
        //    return GetSiteIdsClaimValue(siteidsValue);
        //}

        public static string GetAvaliableRccCodes(AccountEF _accountContext, string userRoleType, string SpecificId)
        {
            IEnumerable<VwContractorsiteEF> results = null;
            if (userRoleType == UserRoleTypes.Supervisor)
            {
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null);//.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value.SiteId);
                                                                                         //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                                                                                         //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                                                                                         //    return
                                                                                         // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (userRoleType == UserRoleTypes.Contractor)
            {
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == SpecificId);
            }
            else if (userRoleType == UserRoleTypes.Aggregator)
            {
                string groupId = SpecificId;
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId);
            }
            var result = results.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value.SiteId);
            List<string> codes = new List<string>();
            foreach (IGrouping<int, int> row in result)
            {
                foreach (int siteId in row)
                {
                    string field = $"{row.Key}:{siteId}";
                    codes.Add(field);
                }
            }
            return string.Join(',', codes);

        }

        public  static IEnumerable<IGrouping<int, int>> GetAvaliableRccCodes(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext)
        {
            IEnumerable<VwContractorsiteEF> results = null;
            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null);//.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value.SiteId);
                                                                                         //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                                                                                         //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                                                                                         //    return
                                                                                         // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(httpContext.User, ClaimTypes.NameIdentifier);
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId);
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId);
            }
            return results.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value.SiteId);

        }

        public static IEnumerable<IGrouping<int, VwContractorsiteEF>> GetAvaliableRccEntities(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext)
        {
            IEnumerable<VwContractorsiteEF> results = null;
            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null).ToArray();//.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value.SiteId);
                                                                                         //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                                                                                         //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                                                                                         //    return
                                                                                         // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(httpContext.User, ClaimTypes.NameIdentifier);
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId);
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId);
            }
            return results.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value);

        }

        public static IEnumerable<int> GetAvaliableSiteIdsByAggGroupId(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext, string AggGroupId)
        {
            IEnumerable<VwContractorsiteEF> results = null;

            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                results = _accountContext.VwContractorsites.Where(x => x.AggGroupId == AggGroupId);
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else
                return null;
            return results.Select(x => x.SiteId).ToArray();
        }

        public  static IEnumerable<int> GetAvaliableSiteIdsByRcc(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext, int Rcc)
        {
            IEnumerable<VwContractorsiteEF> results = null;

            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null);//.OrderBy(x => x.RCC).GroupBy(key => key.RCC, value => value.SiteId);
                                                                                         //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                                                                                         //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                                                                                         //    return
                                                                                         // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(httpContext.User, ClaimTypes.NameIdentifier);
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId);
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                results = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId);
            }
            return results.Where(x => x.RCC == Rcc).Select(x => x.SiteId).ToArray();
        }

        public static bool ValidateSiteId(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext, int siteId)
        {
            bool IsValidate = false;
            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                IsValidate = _accountContext.VwContractorsites.Any(x => x.UserId != null && x.SiteId == siteId);
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(httpContext.User, ClaimTypes.NameIdentifier);
                IsValidate = _accountContext.VwContractorsites.Any(x => x.UserId != null && x.UserId == userId && x.SiteId == siteId);
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                IsValidate = _accountContext.VwContractorsites.Any(x => x.UserId != null && x.AggGroupId == groupId && x.SiteId == siteId);
            }
            return IsValidate;
        }

        public static IEnumerable<int> GetAvaliableSiteIds(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext)
        {
            IEnumerable<int> siteIds = null;
            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null).Select(x => x.SiteId).ToArray();
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(httpContext.User, ClaimTypes.NameIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId).Select(x => x.SiteId).ToArray();
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId).Select(x => x.SiteId).ToArray();
            }
            return siteIds;
        }

        public static IEnumerable<VwContractorsiteEF> GetAvaliableSites(AccountEF _accountContext, IClaimServiceFactory _claimsManager, HttpContext httpContext)
        {
            IEnumerable<VwContractorsiteEF> siteIds = null;
            if (httpContext.User.IsInRole(UserRoleTypes.Supervisor))
            {
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null);
                //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    return
                // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Contractor))
            {
                string userId = _claimsManager.GetClaimsValue(httpContext.User, ClaimTypes.NameIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId);
            }
            else if (httpContext.User.IsInRole(UserRoleTypes.Aggregator))
            {
                string groupId = _claimsManager.GetClaimsValue(httpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                siteIds = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.AggGroupId == groupId);
            }
            return siteIds;
        }
    }
}
