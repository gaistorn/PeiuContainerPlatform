using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    internal interface IRedisStorage
    {
        Task<T> GetValue<T>(string key);
        Task<T> GetValue<T>(string key, string hashKey);
    }
    internal class RedisStorage : IRedisStorage
    {
        private readonly IDatabaseAsync database;
        public RedisStorage(ConnectionMultiplexer connectionMultiplexer)
        {
            database = connectionMultiplexer.GetDatabase(2);
        }

        //public override void SetValue(string Key, IComparable Value)
        //{
        //    base.SetValue(Key, Value);
        //    Semaphore.Release();
        //    //RedisValue redisValue = new RedisValue(Value.ToString());
        //    //Task t = database.StringSetAsync(Key, redisValue);
        //    //t.Wait();
        //}

        public async Task<T> GetValue<T>(string key, string hashKey)
        {
            //await Semaphore.WaitAsync(cancellationToken);
            if (await database.KeyExistsAsync(key))
            {
                RedisValue value = await database.HashGetAsync(key, hashKey);
                string str_value = value.ToString();
                object v = Convert.ChangeType(str_value, typeof(T));
                return (T)v;
            }
            return default(T);
        }

        public async Task<T> GetValue<T>(string key)
        {
            //await Semaphore.WaitAsync(cancellationToken);
            if (await database.KeyExistsAsync(key))
            {
                string str_value = await database.StringGetAsync(key);

                object v = Convert.ChangeType(str_value, typeof(T));
                return (T)v;
            }
            return default(T);
        }
    }
}
