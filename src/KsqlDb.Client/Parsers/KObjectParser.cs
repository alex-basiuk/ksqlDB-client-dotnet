using System;
using System.Text.Json;
using KsqlDb.Api.Client.Abstractions.Objects;
// ReSharper disable HeapView.BoxingAllocation

namespace KsqlDb.Api.Client.Parsers
{
    public class KObjectParser
    {
        private static readonly char[] _nonPrimitiveTypeElements = {'<', '('};
        private readonly Func<JsonElement, object> _parse;

        public Type TargetType { get; }

        private KObjectParser(Func<JsonElement, object> parse, Type targetType) => (_parse, TargetType) = (parse, targetType);

        public object Parse(JsonElement value) => _parse(value);

        public static KObjectParser Create(string type)
        {
            var primaryType = GetPrimaryTypeName(type);
            if (primaryType.Equals("STRING", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(ParseString, typeof(string));
            if (primaryType.Equals("INTEGER", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(ParseInt, typeof(int));
            if (primaryType.Equals("BIGINT", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(ParseLong, typeof(long));
            if (primaryType.Equals("DOUBLE", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(ParseDouble, typeof(double));
            if (primaryType.Equals("BOOLEAN", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(TryParseBoolean, typeof(bool));
            if (primaryType.Equals("DECIMAL", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(ParseDecimal, typeof(decimal));
            if (primaryType.Equals("ARRAY", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(j => ParseArray(j, type), typeof(KSqlArray));
            if (primaryType.Equals("MAP", StringComparison.OrdinalIgnoreCase)) return new KObjectParser(j => ParseMap(j, type), typeof(KSqlObject));
            //TODO Add support of STRUCT
            //if (primaryType.Equals("STRUCT", StringComparison.OrdinalIgnoreCase)) return new StructKObjectParser();
            throw new NotSupportedException($"The {type} type is not supported.");
        }

        private static ReadOnlySpan<char> GetPrimaryTypeName(ReadOnlySpan<char> type)
        {
            int specialSymbolIndex = type.IndexOfAny(_nonPrimitiveTypeElements);
            return specialSymbolIndex == -1 ? type : type.Slice(0, specialSymbolIndex);
        }

        private static object ParseString(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind != JsonValueKind.String) return KSqlNull.Instance;
            object? stringValue = jsonElement.GetString();
            return stringValue ?? KSqlNull.Instance;
        }

        private static object ParseInt(JsonElement jsonElement) => ParseNumber(jsonElement, (JsonElement element, out int output) => element.TryGetInt32(out output));

        private static object ParseLong(JsonElement jsonElement) => ParseNumber(jsonElement, (JsonElement element, out long output) => element.TryGetInt64(out output));

        private static object ParseDouble(JsonElement jsonElement) => ParseNumber(jsonElement, (JsonElement element, out double output) => element.TryGetDouble(out output));

        private static object ParseDecimal(JsonElement jsonElement) => ParseNumber(jsonElement, (JsonElement element, out decimal output) => element.TryGetDecimal(out output));

        private delegate bool TryGetNumber<T>(JsonElement jsonElement, out T output);
        private static object ParseNumber<T>(JsonElement jsonElement, TryGetNumber<T> tryGet) where T : struct
        {
            if (jsonElement.ValueKind != JsonValueKind.Number) return KSqlNull.Instance;
            if (tryGet(jsonElement, out T value)) return value;
            return KSqlNull.Instance;
        }

        private static object TryParseBoolean(JsonElement jsonElement) =>
            jsonElement.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => KSqlNull.Instance
            };

        private static object ParseArray(JsonElement jsonElement, ReadOnlySpan<char> fullTypeName)
        {
            if (jsonElement.ValueKind != JsonValueKind.Array) return KSqlNull.Instance;
            var itemType = fullTypeName.Slice(6, fullTypeName.Length - 7).Trim();
            var itemParser = Create(itemType.ToString());
            var kSqlArray = new KSqlArray();
            foreach (var itemJsonElement in jsonElement.EnumerateArray())
            {
                var item = itemParser.Parse(itemJsonElement);
                kSqlArray.AddValue(item);
            }

            return kSqlArray;
        }

        private static object ParseMap(JsonElement jsonElement, ReadOnlySpan<char> fullTypeName)
        {
            if (jsonElement.ValueKind != JsonValueKind.Object) return KSqlNull.Instance;
            var itemType = fullTypeName.Slice(12, fullTypeName.Length - 13).Trim();
            var valueParser = Create(itemType.ToString());
            var kSqlObject = new KSqlObject();
            foreach (var jsonProperty in jsonElement.EnumerateObject())
            {
                var item = valueParser.Parse(jsonProperty.Value);
                kSqlObject.AddValue(jsonProperty.Name, item);
            }

            return kSqlObject;
        }
    }
}