using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Abstractions.QueryResults;

namespace KsqlDb.Api.Client.Parsers
{
    internal class QueryResultRowParser
    {
        public Dictionary<string, int> ColumnNameToIndex { get; }
        public (string, Type)[] ColumnNamesAndTypes { get; }
        private readonly KObjectParser[] _kObjectParsers;

        public QueryResultRowParser(JsonElement schema)
        {
            if (!schema.TryGetProperty("columnNames", out var columnsNamesElement) ||
                !schema.TryGetProperty("columnTypes", out var columnsTypesElement))
            {
                throw new ArgumentException();
            }

            var columnNamesArray = columnsNamesElement.EnumerateArray().ToArray();
            var columnTypesArray = columnsTypesElement.EnumerateArray().ToArray();

            if (columnNamesArray.Length != columnTypesArray.Length) throw new ArgumentException();
            int columnCount = columnNamesArray.Length;

            _kObjectParsers = new KObjectParser[columnCount];
            ColumnNamesAndTypes = new (string, Type)[columnCount];
            ColumnNameToIndex = new Dictionary<string, int>();

            for (int i = 0; i < columnCount; i++)
            {
                string columnName = columnNamesArray[i].GetString() ?? throw new Exception();
                string columnType = columnTypesArray[i].GetString() ?? throw new Exception();
                ColumnNameToIndex.Add(columnName, i);
                var columnParser = KObjectParser.Create(columnType);
                _kObjectParsers[i] = columnParser;
                ColumnNamesAndTypes[i] = (columnName, columnParser.TargetType);
            }
        }

        public QueryResultRow Parse(JsonElement row)
        {
            if (row.ValueKind != JsonValueKind.Array) throw new Exception();
            var values = new KSqlArray();
            int index = 0;
            foreach (var value in row.EnumerateArray())
            {
                if (index > _kObjectParsers.Length) throw new Exception();
                var parsedValue = _kObjectParsers[index++].Parse(value);
                values.AddValue(parsedValue);
            }

            return new QueryResultRow(values, ColumnNameToIndex, ColumnNamesAndTypes);
        }
    }
}
