using KsqlDb.Api.Client.Abstractions;

namespace KSQL.API.Client
{
    /// <summary>
    /// The result of the <see cref="IClient.ExecuteStatement"/> method.
    /// </summary>
    public class ExecuteStatementResult
    {
        /// <summary>
        /// Returns the ID of a newly started persistent query, if applicable. The return value is empty
        /// for all statements other than 'CREATE ... AS * SELECT' and 'INSERT * INTO ... AS SELECT'
        /// statements, as only these statements start persistent queries. For statements that start
        /// persistent queries, the return value may still be empty if either:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The statement was not executed on the server by the time the server response was sent.
        /// This typically does not happen under normal server operation, but may happen if the ksqlDB
        /// server's command runner thread is stuck, or if the configured value for <code>ksql.server.command.response.timeout.ms</code>
        /// is too low.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The ksqlDB server version is lower than 0.11.0.
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        string? QueryId;

        public ExecuteStatementResult(string? queryId)
        {
            QueryId = queryId;
        }
    }
}
