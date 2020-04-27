using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PEIU.Service
{
    public static class CommonFactory
    {
        public const string PVRedisKeyPattern = "SID*.GID4.EnergyTotalActivePower";
        public static string CreateRedisKey(int siteId, int groupId, params string[] childKeys)
        {
            string child = childKeys.Length > 0 ? "." + string.Join(".", childKeys) : "";
            return string.Format($"SID{siteId}.GID{groupId}{child}");
        }

        public static async Task<JObject> RetriveWeather(int siteid, IDatabaseAsync _redisDb)
        {
            JObject center_weather = new JObject();
            // Weather
            string targetcc_redis_key = $"weather.sid{siteid}";
            if (await _redisDb.KeyExistsAsync(targetcc_redis_key))
            {
                var entries = await _redisDb.HashGetAllAsync(targetcc_redis_key);
                foreach (var entry in entries)
                {
                    center_weather.Add(entry.Name, entry.Value.ToString());
                }
            }
            else return null;
            return center_weather;
        }

        public static IEnumerable<RedisKey> SearchKeys(ConnectionMultiplexer redisConnection, string search_pattern)
        {
            var db = redisConnection.GetDatabase();
            List<RedisKey> entire_keys = new List<RedisKey>();
            foreach (EndPoint endpoint in redisConnection.GetEndPoints())
            {
                IServer server = redisConnection.GetServer(endpoint);
                RedisKey[] Keys = server.Keys(pattern: search_pattern).ToArray();
                entire_keys.AddRange(Keys);
            }
            return entire_keys;
        }

    }
}
