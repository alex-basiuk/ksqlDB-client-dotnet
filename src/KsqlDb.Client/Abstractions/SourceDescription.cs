using System.Collections.Generic;
using KSQL.API.Client;

namespace KsqlDb.Api.Client.Abstractions
{
    /// <summary>
    /// Metadata for a ksqlDB stream or table.
    /// </summary>
    public class SourceDescription
    {
        /// <summary>
        /// The name of this stream or table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of this source.
        /// </summary>
        public SourceType Type { get; }

        /// <summary>
        /// The collection of fields (key and value) present in this stream/table.
        /// </summary>
        public IReadOnlyCollection<FieldInfo> Fields { get; }

        /// <summary>
        /// The name of the Kafka topic underlying this ksqlDB stream/table.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// The serialization formats of the key and the value used in this stream/table.
        /// </summary>
        public (string key, string value) SerializationFormat { get; }

        /// <summary>
        /// The collection of ksqlDB queries currently reading from this stream/table.
        /// </summary>
        public IReadOnlyCollection<QueryInfo> ReadQueries { get; }

        /// <summary>
        /// The collection of ksqlDB queries currently writing to this stream/table.
        /// </summary>
        public IReadOnlyCollection<QueryInfo> WriteQueries { get; }

        /// <summary>
        /// The name of the column configured as the <code>TIMESTAMP</code> for this stream/table, if any.
        /// </summary>
        public string? TimestampColumnName { get; }

        /// <summary>
        /// The type of the window (e.g., "TUMBLING", "HOPPING", "SESSION") associated with this source, if this source is a windowed table. Else, empty.
        /// </summary>
        public string? WindowType { get; }

        /// <summary>
        /// Returns the ksqlDB statement text used to create this stream/table. This text may not be
        /// exactly the statement submitted in order to create this stream/table, but submitting this
        /// statement will result in exactly this stream/table being created.
        /// </summary>
        public string SqlStatement { get; }

        public SourceDescription(string name, SourceType type, IReadOnlyCollection<FieldInfo> fields, string topic, (string key, string value) serializationFormat, IReadOnlyCollection<QueryInfo> readQueries, IReadOnlyCollection<QueryInfo> writeQueries, string? timestampColumnName, string? windowType, string sqlStatement)
        {
            Name = name;
            Type = type;
            Fields = fields;
            Topic = topic;
            SerializationFormat = serializationFormat;
            ReadQueries = readQueries;
            WriteQueries = writeQueries;
            TimestampColumnName = timestampColumnName;
            WindowType = windowType;
            SqlStatement = sqlStatement;
        }
    }
}
