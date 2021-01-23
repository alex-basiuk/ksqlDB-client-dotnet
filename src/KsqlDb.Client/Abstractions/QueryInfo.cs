namespace KSQL.API.Client
{
    /// <summary>
    /// Metadata for a ksqlDB query.
    /// </summary>
    public class QueryInfo
    {
        /// <summary>
        /// The type of this query
        /// </summary>
        QueryType QueryType { get; }

        /// <summary>
        /// The ID of this query, used for control operations such as terminating the query.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Returns the ksqlDB statement text corresponding to this query. This text may not be exactly the
        /// statement submitted in order to start the query, but submitting this statement will result
        /// in exactly this query.
        /// </summary>
        string Sql { get; }

        /// <summary>
        /// Returns the ksqlDB stream or table sink and the underlying topic that this query writes to, if this query is
        /// persistent. If this query is a push query, then the value is not set.
        /// </summary>
        (string name, string topic)? Sink { get; }

        public QueryInfo(QueryType queryType, string id, string sql, (string name, string topic)? sink)
        {
            QueryType = queryType;
            Id = id;
            Sql = sql;
            Sink = sink;
        }
    }
}
