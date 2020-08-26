using PeiuPlatform.App;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    internal class RedisStorage : GlobalStorage<EventModel>
    {
        private readonly IDatabaseAsync database;
        public RedisStorage(ConnectionMultiplexer connectionMultiplexer)
        {
            database = connectionMultiplexer.GetDatabase(2);
        }

        public override void SetValue(string Key, IComparable Value)
        {
            base.SetValue(Key, Value);
            RedisValue redisValue = new RedisValue(Value.ToString());
            Task t = database.StringSetAsync(Key, redisValue);
            t.Wait();
        }

        public override async Task<IComparable> GetValue(string key, CancellationToken cancellationToken)
        {
            if(base.ContainKey(key))
                return await base.GetValue(key, cancellationToken);
            else if(await database.KeyExistsAsync(key))
            {
                string str_value = await database.StringGetAsync(key);
                if (int.TryParse(str_value, out int iResult))
                    return iResult;
                else if (float.TryParse(str_value, out float fresult))
                    return fresult;
            }
            return 0;
        }
    }
}
