using System.Diagnostics;

namespace KsqlDb.Api.Client.Abstractions
{
    /// <summary>
    /// Metadata for a ksqlDB stream.
    /// </summary>
    [DebuggerDisplay("Name={" + nameof(Name) + "}")]
    public class StreamOrTableInfo
    {
        /// <summary>
        /// The name of this stream or table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the Kafka topic underlying this ksqlDB stream or table.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// The key format of the data in this stream or table.
        /// </summary>
        public string KeyFormat { get; }

        /// <summary>
        /// The value format of the data in this stream or table.
        /// </summary>
        public string ValueFormat { get; }

        /// <summary>
        /// Whether the key is windowed.
        /// </summary>
        public bool? IsWindowed { get; }

        public StreamOrTableInfo(string name, string topic, string keyFormat, string valueFormat, bool? isWindowed = null)
        {
            Name = name;
            Topic = topic;
            KeyFormat = keyFormat;
            ValueFormat = valueFormat;
            IsWindowed = isWindowed;
        }
    }
}
