using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace KsqlDb.Api.Client.Abstractions.Objects
{
    public class KSqlArray
    {
        private readonly List<object> _items;

        public KSqlArray() : this(new List<object>())
        {
        }

        private KSqlArray(List<object> items) => _items = items;

        public int Count => _items.Count;

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public object this[int index] => _items[index];

        /// <summary>
        /// Returns the value at a specified index as a <see cref="string"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value at <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="index"/> is invalid.</exception>
        /// <exception cref="InvalidCastException">If the value is not a <see cref="string"/>.</exception>
        public string GetString(int index) => GetValue<string>(index);
        public int GetInteger(int index) => GetValue<int>(index);
        public long GetLong(int index) => GetValue<long>(index);
        public double GetDouble(int index) => GetValue<double>(index);
        public decimal GetDecimal(int index) => GetValue<decimal>(index);
        public KSqlArray GetKSqlArray(int index) => GetValue<KSqlArray>(index);
        public KSqlObject GetKSqlObject(int index) => GetValue<KSqlObject>(index);
        public bool IsNull(int index) => _items[index] is KSqlNull;

        public bool Remove(object value) => _items.Remove(value);
        public void RemoveAt(int index) => _items.RemoveAt(index);

        public KSqlArray Add(string value) => AddValue(value);
        public KSqlArray Add(int value) => AddValue(value);
        public KSqlArray Add(long value) => AddValue(value);
        public KSqlArray Add(double value) => AddValue(value);
        public KSqlArray Add(decimal value) => AddValue(value);
        public KSqlArray Add(KSqlArray value) => AddValue(value);
        public KSqlArray Add(KSqlObject value) => AddValue(value);
        public KSqlArray AddNull() => AddValue(KSqlNull.Instance);
        public KSqlArray AddRange(KSqlArray array)
        {
            _items.AddRange(array._items);
            return this;
        }

        /// <summary>
        /// Shallow copy.
        /// </summary>
        /// <returns></returns>
        public KSqlArray Copy() => new KSqlArray(_items);

        public IReadOnlyList<object> AsReadOnlyList() => ImmutableArray.CreateRange(_items);

        private T GetValue<T>(int index)
        {
            var item = _items[index];
            if (_items[index] is T value) return value;
            throw new InvalidCastException($"The value at index {index} is not a {typeof(T).FullName}, it's a {item.GetType().FullName}");
        }

        internal KSqlArray AddValue(object value)
        {
            _items.Add(value);
            return this;
        }
    }
}
