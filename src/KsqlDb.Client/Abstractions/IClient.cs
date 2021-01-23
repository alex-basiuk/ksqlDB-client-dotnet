using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using KSQL.API.Client;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Abstractions.QueryResults;

namespace KsqlDb.Api.Client.Abstractions
{
    public interface IClient
    {
        /// <summary>
        /// Executes a pull or push query supplied in the <paramref name="sql"/> and streams the result.
        /// </summary>
        /// <param name="sql">The query statement.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the operation.</param>
        /// <returns>
        /// A task that completes when the server response is received.
        /// </returns>
        Task<StreamedQueryResult> StreamQuery(string sql, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a pull or push query supplied in the <paramref name="sql"/> and streams the result.
        /// </summary>
        /// <param name="sql">The query statement.</param>
        /// <param name="properties">The query properties.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the operation.</param>
        /// <returns>
        /// A task that completes when the server response is received.
        /// </returns>
        Task<StreamedQueryResult> StreamQuery(string sql, IDictionary<string, object> properties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a pull or push query supplied in the <paramref name="sql"/> and returns the result as a single batch or rows.
        /// </summary>
        /// <param name="sql">The query statement.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the operation.</param>
        /// <returns>
        /// A task that completes when the server response is received.
        /// </returns>
        Task<BatchedQueryResults> BatchQuery(string sql, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a pull or push query supplied in the <paramref name="sql"/> and returns the result as a single batch or rows.
        /// </summary>
        /// <param name="sql">The query statement.</param>
        /// <param name="properties">The query properties.</param>
        /// <param name="cancellationToken">The cancellation token that cancels the operation.</param>
        /// <returns>
        /// A task that completes when the server response is received.
        /// </returns>
        Task<BatchedQueryResults> BatchQuery(string sql, IDictionary<string, object> properties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminates a push query with the specified <paramref name="queryId"/>,
        /// </summary>
        /// <param name="queryId">The Id of the query to terminate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that completes once the server response is received.</returns>
        Task TerminatePushQuery(string queryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a new <paramref name="row"/> into the <param name="streamName"></param>
        /// </summary>
        /// <param name="streamName">The target stream name.</param>
        /// <param name="row">The row to be inserted into the stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        Task<RowInsertAcknowledgment> InsertRow(string streamName, KSqlObject row, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts the asynchronous stream of rows <paramref name="rows"/> in to the stream <paramref name="streamName"/>.
        /// </summary>
        /// <param name="streamName">The target stream name.</param>
        /// <param name="rows">The asynchronous stream of rows.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asynchronous stream of row acknowledgments.</returns>
        IAsyncEnumerable<RowInsertAcknowledgment> StreamInserts(string streamName, IAsyncEnumerable<KSqlObject> rows, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a DDL/DML statement to the ksqlDB server.
        /// This method supports the following statements:
        /// - 'CREATE';
        /// - 'CREATE ... AS SELECT';
        /// - 'DROP', 'TERMINATE';
        /// - 'INSERT INTO ... AS SELECT'.
        /// Each request should contain exactly one statement. Requests that contain multiple statements will be rejected by the client.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        Task<ExecuteStatementResult> ExecuteStatement(string sql, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a DDL/DML statement to the ksqlDB server.
        /// This method supports the following statements:
        /// - 'CREATE';
        /// - 'CREATE ... AS SELECT';
        /// - 'DROP', 'TERMINATE';
        /// - 'INSERT INTO ... AS SELECT'.
        /// Each request should contain exactly one statement. Requests that contain multiple statements will be rejected by the client.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="properties">The statement properties.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        Task<ExecuteStatementResult> ExecuteStatement(string sql, IDictionary<string, object> properties, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the list of ksqlDB streams from the ksqlDB server's metastore.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that returns a collection of streams when completes.</returns>
        Task<IReadOnlyCollection<StreamOrTableInfo>> ListStreams(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the list of ksqlDB tables from the ksqlDB server's metastore.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that returns a collection of tables when completes.</returns>
        Task<IReadOnlyCollection<StreamOrTableInfo>> ListTables(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the list of Kafka topics  available for use with ksqlDB.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that returns a collection of topics when completes.</returns>
        Task<IReadOnlyCollection<TopicInfo>> ListTopics(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the list of queries currently running on the ksqlDB server.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that returns a collection of queries when completes.</returns>
        Task<IReadOnlyCollection<QueryInfo>> ListQueries(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns metadata about the ksqlDB stream or table of the provided name.
        /// </summary>
        /// <param name="sourceName">The stream or table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that returns metadata for the stream or table.</returns>
        Task<SourceDescription> DescribeSource(string sourceName, CancellationToken cancellationToken = default);
    }
}
