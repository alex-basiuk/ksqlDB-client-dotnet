using System;
using System.Collections.Generic;

namespace KsqlDb.Api.Client.KsqlApiV1.Requests
{
    /// <summary>
    /// The ksql request.
    /// </summary>
    internal class KsqlRequest
    {
        /// <summary>
        /// The semicolon-delimited sequence of SQL statements to run.
        /// </summary>
        public string Ksql { get; }

        /// <summary>
        /// The property overrides to run the statements with. Refer to the <a href="https://docs.ksqldb.io/en/latest/operate-and-deploy/installation/server-config/config-reference">Configuration Parameter Reference</a> for details.
        /// </summary>
        public IDictionary<string, object>? StreamsProperties { get; }

        /// <summary>
        /// The optional command sequence number.
        /// If specified, the statements will not be run until all existing commands up to and including the specified sequence number have completed.
        /// If unspecified, the statements are run immediately. When a command is processed, the result object contains its sequence number.
        /// </summary>
        public long? CommandSequenceNumber { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KsqlRequest"/> class.
        /// </summary>
        /// <param name="ksql">The semicolon-delimited sequence of SQL statements to run.</param>
        /// <param name="streamsProperties">The optional property overrides to run the statements with.</param>
        /// <param name="commandSequenceNumber">The optional command sequence number.</param>
        public KsqlRequest(string? ksql, IDictionary<string, object>? streamsProperties, long? commandSequenceNumber)
        {
            Ksql = !string.IsNullOrWhiteSpace(ksql) ? ksql : throw new ArgumentNullException(nameof(ksql));
            StreamsProperties = streamsProperties;
            CommandSequenceNumber = commandSequenceNumber;
        }
    }
}
