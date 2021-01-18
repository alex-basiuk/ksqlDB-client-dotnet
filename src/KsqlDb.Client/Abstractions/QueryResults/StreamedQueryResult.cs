using System;
using System.Collections.Generic;

namespace KsqlDb.Api.Client.Abstractions.QueryResults
{
    /// <summary>
    /// A streamed query result.
    /// </summary>
    public class StreamedQueryResult
    {
        /// <summary>
        /// The column names and types.
        /// </summary>
        public IReadOnlyCollection<(string name, Type type)> Columns { get; }

        /// <summary>
        /// The asynchronous stream of result rows.
        /// </summary>
        public IAsyncEnumerable<QueryResultRow> Rows { get; }

        /// <summary>
        /// The Id of the underlying push query if applicable.
        /// </summary>
        public string? QueryId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamedQueryResult"/> class.
        /// </summary>
        /// <param name="columns">The column names and types.</param>
        /// <param name="resultRows">The async stream of result rows.</param>
        /// <param name="queryId">The optional query Id.</param>
        public StreamedQueryResult(IReadOnlyCollection<(string name, Type type)> columns,
                                   IAsyncEnumerable<QueryResultRow> resultRows,
                                   string? queryId)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Rows = resultRows ?? throw new ArgumentNullException(nameof(resultRows));
            QueryId = queryId;
        }
    }
}
