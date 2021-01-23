using System.Collections.Generic;
using System.Text.Json.Serialization;
#nullable disable

namespace KsqlDb.Api.Client.KsqlApiV1.Responses
{
    /// <summary>
    /// The /ksql endpoint response.
    /// </summary>
    internal class KsqlResponse
    {
        // Fields that are common to all responses
        [JsonPropertyName("@type")]
        public string Type { get; set; }
        public string StatementText { get; set; }

        public WarningMessage[] Warnings { get; set; }

        // CREATE, DROP and TERMINATE fields
        public string CommandId  { get; set; }
        public CommandInfo CommandStatus { get; set; }
        public long CommandSequenceNumber { get; set; }

        // LIST STREAMS and SHOW STREAMS fields
        public StreamOrTableInfo[] Streams { get; set; }

        // LIST TABLES, SHOW TABLES fields
        public StreamOrTableInfo[] Tables { get; set; }

        // LIST QUERIES, SHOW QUERIES fields
        public QueryInfo[] Queries { get; set; }

        // LIST TOPICS QUERIES fields
        public TopicInfo[] Topics { get; set; }

        // LIST PROPERTIES, SHOW PROPERTIES fields
        public Dictionary<string, string> Properties { get; set; }

        internal class WarningMessage
        {
            public string Warning { get; set; }
        }

        internal class CommandInfo
        {
            public string Status { get; set; }
            public string Message  { get; set; }
        }

        internal class StreamOrTableInfo
        {
            public string Name { get; set; }
            public string Topic { get; set; }
            public string Format { get; set; }
            public string Type { get; set; }
            public bool? IsWindowed { get; set; }
        }

        internal class QueryInfo
        {
            public string QueryString { get; set; }
            public string Sinks { get; set; }
            public string Id { get; set; }
        }

        internal class TopicInfo
        {
            public string Name { get; set; }
            public int[] ReplicaInfo { get; set; }
        }
    }
}
