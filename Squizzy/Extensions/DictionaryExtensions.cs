using System;
using System.Collections.Generic;

namespace Squizzy.Extensions
{
    public static partial class Extensions
    {
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey,TValue> addValueFactory, Func<TKey, TValue, TValue> updateFactory)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, addValueFactory.Invoke(key));
            }
            else
            {
                dictionary.Remove(key, out var oldValue);
                dictionary.Add(key, updateFactory.Invoke(key, oldValue));
            }
        }

        public static void TryUpdate<TKey,TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue, TValue> updateFactory)
        {
            if (!dictionary.Remove(key, out var oldValue))
            {
                return;
            }
            else
            {
                dictionary.Add(key, updateFactory(key, oldValue));
            }
        }
    }
}
