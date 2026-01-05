using Data;

namespace Application.Extentions
{
    public static class DictionaryExtentions
    {
        public static void AddPair<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> kvp) => dict.TryAdd(kvp.Key, kvp.Value);


        public static TValue GetValueSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            return (TValue)(object)DataDefaultValues.Null;
        }
    }
}
