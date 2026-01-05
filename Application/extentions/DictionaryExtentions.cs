using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extentions
{
    public static class DictionaryExtentions
    {
        public static void AddPair<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> kvp)
        {
            if (!dict.ContainsKey(kvp.Key))
            {
                dict.Add(kvp.Key, kvp.Value);
            }
        }

        public static TValue GetValueSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return (TValue)(object)DataDefaultValues.Null;
        }

        /// <summary>
        /// Добавляет значение к существующему ключу, либо создаёт новый ключ.
        /// </summary>
        public static void SumToKey<TKey>(this Dictionary<TKey, int> dict, TKey key, int value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] += value;
            }
            else
            {
                dict[key] = value;
            }
        }

        public static void SumToKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.TryGetValue(key, out var existing))
            {
                dict[key] = (dynamic)existing + (dynamic)value;
            }
            else
            {
                dict[key] = value;
            }
        }
    }
}
