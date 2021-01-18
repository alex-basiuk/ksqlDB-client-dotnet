using System;
using System.Collections.Generic;
using System.Linq;
using KsqlDb.Api.Client.Abstractions.Objects;

namespace KsqlDb.Api.Client.Abstractions.QueryResults
{
    public class QueryResultRow
    {
        private readonly KSqlArray _values;
        private readonly Dictionary<string, int> _columnNameToIndex;
        public IReadOnlyList<(string name, Type type)> ColumnNamesAndTypes { get; }

        public QueryResultRow(KSqlArray values, Dictionary<string, int> columnNameToIndex, IReadOnlyList<(string, Type)> columnNamesAndTypes)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
            _columnNameToIndex = columnNameToIndex ?? throw new ArgumentNullException(nameof(columnNameToIndex));
            ColumnNamesAndTypes = columnNamesAndTypes ?? throw new ArgumentNullException(nameof(columnNamesAndTypes));
        }

        public KSqlArray AsArray => _values.Copy();

        public KSqlObject AsObject
        {
            get
            {
                string[] fieldNames = ColumnNamesAndTypes.Select(t => t.name).ToArray();
                return KSqlObject.FromArray(fieldNames, _values);
            }
        }

        public object this[int columnIndex] => _values[AdjustIndex(columnIndex)];
        public object this[string columnName] => _values[IndexFromName(columnName)];

        public string GetString(int columnIndex) => _values.GetString(AdjustIndex(columnIndex));
        public string GetString(string columnName) => _values.GetString(IndexFromName(columnName));
        public int GetInteger(int columnIndex) => _values.GetInteger(AdjustIndex(columnIndex));
        public int GetInteger(string columnName) => _values.GetInteger(IndexFromName(columnName));
        public long GetLong(int columnIndex) => _values.GetLong(AdjustIndex(columnIndex));
        public long GetLong(string columnName) => _values.GetLong(IndexFromName(columnName));
        public double GetDouble(int columnIndex) => _values.GetDouble(AdjustIndex(columnIndex));
        public double GetDouble(string columnName) => _values.GetDouble(IndexFromName(columnName));
        public decimal GetDecimal(int columnIndex) => _values.GetDecimal(AdjustIndex(columnIndex));
        public decimal GetDecimal(string columnName) => _values.GetDecimal(IndexFromName(columnName));
        public KSqlArray GetKSqlArray(int columnIndex) => _values.GetKSqlArray(AdjustIndex(columnIndex));
        public KSqlArray GetKSqlArray(string columnName) => _values.GetKSqlArray(IndexFromName(columnName));
        public KSqlObject GetKSqlObject(int columnIndex) => _values.GetKSqlObject(AdjustIndex(columnIndex));
        public KSqlObject GetKSqlObject(string columnName) => _values.GetKSqlObject(IndexFromName(columnName));
        public bool IsNull(int columnIndex) => _values.IsNull(AdjustIndex(columnIndex));
        public bool IsNull(string columnName) => _values.IsNull(IndexFromName(columnName));

        private int IndexFromName(string columnName) =>
            _columnNameToIndex.TryGetValue(columnName, out int columnIndex)
            ? columnIndex
            : throw new ArgumentException($"No column exists with name: {columnName}");

        private int AdjustIndex(int columnIndex)
        {
            if (columnIndex <= 1) throw new ArgumentException($"Column index cannot be less than 1. The supplied value is {columnIndex}", nameof(columnIndex));
            if (columnIndex > _values.Count) throw new ArgumentException($"Column index cannot be greater than number of columns which is {_values.Count}. The supplied value is {columnIndex}", nameof(columnIndex));
            return columnIndex - 1;
        }
    }
}
