using ModdingManagerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Extentions
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
    }
}
