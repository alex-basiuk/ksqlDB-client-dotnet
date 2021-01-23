namespace KsqlDb.Api.Client.KsqlApiV1.Requests
{
    /// <summary>
    /// The close stream request.
    /// </summary>
    internal class CloseStreamRequest
    {
        /// <summary>
        /// The query Id.
        /// </summary>
        public string? QueryId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloseStreamRequest"/> class.
        /// </summary>
        /// <param name="queryId">The query Id.</param>
        public CloseStreamRequest(string? queryId) => QueryId = queryId;
    }
}
