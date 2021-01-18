using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace KsqlDb.Api.Client.Abstractions.Objects
{
    public class KSqlObject
    {
        private readonly Dictionary<string, object> _map;

        public KSqlObject() : this(new Dictionary<string, object>())
        {
        }

        private KSqlObject(Dictionary<string, object> map) => _map = map;

        public static KSqlObject FromArray(IList<string> keys, KSqlArray values)
        {
            if (keys is null) throw new ArgumentNullException(nameof(keys));
            if (values is null) throw new ArgumentNullException(nameof(values));

            if (keys.Count != values.Count) throw new ArgumentException($"Size of {nameof(keys)} and {nameof(values)} must match.");

            var result = new KSqlObject();
            for (int i = 0; i < keys.Count; i++)
            {
                result.AddValue(keys[i], values[i]);
            }

            return result;
        }

        public int Count => _map.Count;

        public IReadOnlyCollection<string> FieldNames => _map.Keys;

        public object this[string key] => _map[key];

        public bool ContainsKey(string key) => _map.ContainsKey(key);

        public string? TryGetString(string key) => TryGetValue<string>(key);
        public int? TryGetInteger(string key) => TryGetValue<int>(key);
        public long? TryGetLong(string key) => TryGetValue<long>(key);
        public double? TryGetDouble(string key) => TryGetValue<double>(key);
        public decimal? TryGetDecimal(string key) => TryGetValue<decimal>(key);
        public KSqlArray? TryGetKSqlArray(string key) => TryGetValue<KSqlArray>(key);
        public KSqlObject? TryGetKSqlObject(string key) => TryGetValue<KSqlObject>(key);
        public bool IsNull(string key) => _map[key] is KSqlNull;

        public bool Remove(string key) => _map.Remove(key);

        public KSqlObject Add(string key, string value) => AddValue(key, value);
        public KSqlObject Add(string key, int value) => AddValue(key, value);
        public KSqlObject Add(string key, long value) => AddValue(key, value);
        public KSqlObject Add(string key, double value) => AddValue(key, value);
        public KSqlObject Add(string key, decimal value) => AddValue(key, value);
        public KSqlObject Add(string key, bool value) => AddValue(key, value);
        public KSqlObject Add(string key, KSqlArray value) => AddValue(key, value);
        public KSqlObject Add(string key, KSqlObject value) => AddValue(key, value);
        public KSqlObject AddNull(string key) => AddValue(key, KSqlNull.Instance);

        public KSqlObject MergeIn(KSqlObject other)
        {
            foreach (var (key, value) in other._map)
            {
                _map.Add(key, value);
            }

            return this;
        }

        /// <summary>
        /// Shallow copy.
        /// </summary>
        /// <returns></returns>
        public KSqlObject Copy() => new KSqlObject(_map);

        public ImmutableDictionary<string, object> AsImmutableDictionary() => ImmutableDictionary.CreateRange(_map);

        private T? TryGetValue<T>(string key)
        {
            if (!_map.TryGetValue(key, out object? value)) return default;
            if (value is T targetValue) return targetValue;
            throw new InvalidCastException($"The value associated with \"{key}\" key is not a {typeof(T).FullName}, it's a {value.GetType().FullName}");
        }

        internal KSqlObject AddValue(string key, object value)
        {
            _map.Add(key, value);
            return this;
        }
    }
}
