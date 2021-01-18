using System.Collections.Generic;
using System.Diagnostics;

namespace KsqlDb.Api.Client.Abstractions
{
    /// <summary>
    /// Metadata for a Kafka topic available for use with ksqlDB.
    /// </summary>
    [DebuggerDisplay("Topic={" + nameof(Topic) + "}")]
    public class TopicInfo
    {
        /// <summary>
        /// The name of this topic.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// The number of replicas for each topic partition.
        /// </summary>
        public int NumberOfPartitions => ReplicasPerPartition.Count;

        /// <summary>
        /// The number of replicas for each topic partition.
        /// </summary>
        /// <value>
        /// A dictionary where the key represents a partition index and the value represents the corresponding number of replicas.
        /// </value>
        /// <remarks>
        /// The size of the dictionary is equal to <see cref="NumberOfPartitions"/>.
        /// </remarks>
        public IReadOnlyCollection<int> ReplicasPerPartition { get; }

        public TopicInfo(string topic, IReadOnlyCollection<int> replicasPerPartition)
        {
            Topic = topic;
            ReplicasPerPartition = replicasPerPartition;
        }
    }
}
