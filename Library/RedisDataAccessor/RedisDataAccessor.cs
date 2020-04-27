using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PeiuPlatform.DataAccessor
{
    public interface IRedisDataAccessor
    {
        ConnectionMultiplexer Connection();
        IDatabase GetDatabase(int db = -1);
    }

    public class RedisDataAccessor : IRedisDataAccessor
    {
        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connection;

        //public RedisDataAccessor(IConfiguration config)
        //{
        //    var redis_config = config.GetSection("redis").Get<RedisConfiguration>();
        //    this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redis_config.ConfigurationOptions));
        //}

        public RedisDataAccessor(RedisConfiguration config)
        {
            //Console.WriteLine(redis.Value.ConfigurationOptions.ToString());
            this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(config.ConfigurationOptions));
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection.Value;
        }

        public IDatabase GetDatabase(int db = -1) => Connection().GetDatabase(db);

        public static object ConvertToValue(RedisValue value, Type ValueType)
        {
            if (ValueType.IsEnum)
            {
                return Enum.Parse(ValueType, value.ToString());
            }
            else if (ValueType == typeof(string))
                return value.ToString();
            else if (ValueType == typeof(bool))
            {
                return value == "1";
            }
            else
            {
                return Convert.ChangeType(value.ToString(), ValueType);
            }
        }

        public static RedisValue ConvertToRedisValue(object value)
        {
            Type t = value.GetType();
            if (value is int)
                return (int)value;
            else if (value is bool)
                return (bool)value;
            else if (value is double)
                return (double)value;
            else if (value is float)
                return (float)value;
            else
                return value.ToString();
        }

        public static List<HashEntry> CreateHashEntries(object obj, params HashEntry[] includehashEntries)
        {
            List<HashEntry> hashEntries = CreateHashEntries(obj);
            hashEntries.AddRange(includehashEntries);
            return hashEntries;
        }

        public static List<HashEntry> CreateHashEntries(object obj, string Alias = "")
        {
            //List<HashEntry> list = new L
            List<HashEntry> list = new List<HashEntry>();
            FieldInfo[] properties = obj.GetType().GetFields();
            foreach (FieldInfo pi in properties)
            {
                object Value = pi.GetValue(obj);
                if (Value == null)
                    continue;
                Type tValue = Value.GetType();
                string keyName = string.IsNullOrEmpty(Alias) ? pi.Name : Alias + "." + pi.Name;
                if (tValue.IsArray)
                {
                    Array arValue = (Array)Value;
                    for (int i = 0; i < arValue.Length; i++)
                    {
                        HashEntry newArrayFields = new HashEntry(keyName + $"[{i}]", ConvertToRedisValue(arValue.GetValue(i)));
                        list.Add(newArrayFields);
                    }
                }
                else if (tValue.IsValueType && tValue.IsPrimitive == false && Value is string == false)
                {

                    list.AddRange(CreateHashEntries(Value, keyName));
                }
                else
                {
                    list.Add(new HashEntry(keyName, ConvertToRedisValue(Value)));
                }
            }
            return list;
        }

        //public static List<HashEntry> CreateHashEntries(object obj, string Alias = "")
        //{
        //    //List<HashEntry> list = new L
        //    List<HashEntry> list = new List<HashEntry>();
        //    PropertyInfo[] properties = obj.GetType().GetProperties();
        //    foreach (PropertyInfo pi in properties)
        //    {
        //        object Value = pi.GetValue(obj);
        //        if (Value == null)
        //            continue;
        //        Type tValue = Value.GetType();
        //        string keyName = string.IsNullOrEmpty(Alias) ? pi.Name : Alias + "." + pi.Name;
        //        if (tValue.IsClass && Value is string == false)
        //        {

        //            list.AddRange(CreateHashEntries(Value, keyName));
        //        }
        //        else
        //        {
        //            list.Add(new HashEntry(keyName, ConvertToRedisValue(Value)));
        //        }
        //    }
        //    return list;
        //}
    }
}
