using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Hubbub
{
    public class TwoPairKeyHashCube<Key1, Key2, TValue>
    {
        ConcurrentDictionary<Key1, ConcurrentDictionary<Key2, TValue>> map =
            new ConcurrentDictionary<Key1, ConcurrentDictionary<Key2, TValue>>();

        public ConcurrentDictionary<Key2, TValue> this[Key1 k1]
        {
            get
            {
                return map[k1];
            }
        }

        public TValue this[Key1 k1, Key2 k2]
        {
            get
            {
                return GetValueOrDefault(k1, k2);
            }
            set
            {
                AddValueOrUpdate(k1, k2, value);
            }
        }

        public ICollection<Key1> GetKeys() => map.Keys;
        public ICollection<Key2> GetKeys(Key1 firstkey) => map.ContainsKey(firstkey) ? map[firstkey].Keys : throw new KeyNotFoundException();

        public bool ContainsKey(Key1 k1)
        {
            return map.ContainsKey(k1);
        }

        public bool ContainsKey(Key1 k1, Key2 k2)
        {
            return map.ContainsKey(k1) && map[k1].ContainsKey(k2);
        }

        public TValue GetValueOrDefault(Key1 k1, Key2 k2)
        {
            if (ContainsKey(k1, k2))
                return map[k1][k2];
            else
                return default(TValue);
        }

        public bool AddValueOrUpdate(Key1 k1, Key2 k2, TValue value)
        {
            
            if (map.ContainsKey(k1) == false)
                map.TryAdd(k1, new ConcurrentDictionary<Key2, TValue>());
            bool HasChanged = true;
            map[k1].AddOrUpdate(k2, value, (key, oldValue) =>
            {
                HasChanged = !oldValue.Equals(value);
                return value;
                //return oldValue;
            });
            return HasChanged;
            //if (map[k1].ContainsKey(k2) == false)
            //    map[k1].Add(k2, value);
            //else
            //    map[k1][k2] = value;
        }
    }
    public class ThreeKeyHashCube<Key1,Key2,Key3,TValue>
    {
        ConcurrentDictionary<Key1, ConcurrentDictionary<Key2, ConcurrentDictionary<Key3, TValue>>> map =
            new ConcurrentDictionary<Key1, ConcurrentDictionary<Key2, ConcurrentDictionary<Key3, TValue>>>();

        public TValue this[Key1 k1, Key2 k2, Key3 k3]
        {
            get
            {
                return GetValueOrDefault(k1, k2, k3);
            }
            set
            {
                AddValueOrUpdate(k1, k2, k3, value);
            }
        }

        public ConcurrentDictionary<Key3, TValue> this[Key1 k1, Key2 k2]
        {
            get
            {
                return map[k1][k2];
            }
        }

        public ICollection<Key1> GetKeys() => map.Keys;
        public ICollection<Key2> GetKeys(Key1 firstkey) => map.ContainsKey(firstkey) ? map[firstkey].Keys : throw new KeyNotFoundException();
        public ICollection<Key3> GetKeys(Key1 firstkey, Key2 secondKey) => map.ContainsKey(firstkey) && map[firstkey].ContainsKey(secondKey) ? map[firstkey][secondKey].Keys : throw new KeyNotFoundException();

        public bool ContainKeys(Key1 k1, Key2 k2)
        {
            return map.ContainsKey(k1) && map[k1].ContainsKey(k2);
        }

        public bool ContainKeys(Key1 k1, Key2 k2, Key3 k3)
        {
            return ContainKeys(k1, k2) && map[k1][k2].ContainsKey(k3);
        }

        public TValue GetValueOrDefault(Key1 k1, Key2 k2, Key3 k3)
        {            
            if (ContainKeys(k1, k2, k3))
                return map[k1][k2][k3];
            else
                return default(TValue);
        }

        public void AddValueOrUpdate(Key1 k1, Key2 k2, Key3 k3, TValue value)
        {
            
            if (map.ContainsKey(k1) == false)
                map.TryAdd(k1, new ConcurrentDictionary<Key2, ConcurrentDictionary<Key3, TValue>>());
            if (map[k1].ContainsKey(k2) == false)
                map[k1].TryAdd(k2, new ConcurrentDictionary<Key3, TValue>());
            if (map[k1][k2].ContainsKey(k3) == false)
                map[k1][k2].TryAdd(k3, value);
            else
                map[k1][k2][k3] = value;
        }
    }
}
