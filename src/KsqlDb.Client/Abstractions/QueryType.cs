namespace KSQL.API.Client
{
    /// <summary>
    /// A type of the ksqlDB running query.
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// The persistent query.
        /// </summary>
        Persistent,

        /// <summary>
        /// The push query.
        /// </summary>
        Push
    }
}
