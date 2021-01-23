using KsqlDb.Api.Client.Abstractions;

namespace KSQL.API.Client
{
    /// <summary>
    /// A field/column of a ksqlDB stream/table.
    /// </summary>
    public class FieldInfo
    {
        /// <summary>
        /// The field name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The field type.
        /// </summary>
        public ColumnType Type { get; }

        /// <summary>
        /// Whether this field is a key field, rather than a value field.
        /// </summary>
        public bool IsKey { get; }

        public FieldInfo(string name, ColumnType type, bool isKey)
        {
            Name = name;
            Type = type;
            IsKey = isKey;
        }
    }
}
