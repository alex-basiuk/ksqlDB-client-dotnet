using System.Collections.Generic;
using KsqlDb.Api.Client.Abstractions.QueryResults;

namespace KsqlDb.Api.Client.Abstractions
{
    /// <summary>
    /// A batches query result.
    /// </summary>
    public class BatchedQueryResults
    {
        /// <summary>
        /// The result rows async enumerator.
        /// </summary>
        IReadOnlyCollection<QueryResultRow> ResultRows { get; }

        /// <summary>
        /// The Id of the underlying push query if applicable.
        /// </summary>
        public string? QueryId { get; }

        public BatchedQueryResults(IReadOnlyCollection<QueryResultRow> resultRows, string? queryId)
        {
            ResultRows = resultRows;
            QueryId = queryId;
        }
    }
}
