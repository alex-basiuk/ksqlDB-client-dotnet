using System;
using System.Collections.Generic;

namespace KsqlDb.Api.Client.KsqlApiV1.Requests
{
    /// <summary>
    /// The query stream request.
    /// </summary>
    internal class QueryStreamRequest
    {
        /// <summary>
        /// The SELECT statement to run.
        /// </summary>
        public string Sql { get; }

        /// <summary>
        /// The property overrides to run the statements with. Refer to the Config Reference for details on properties that you can set.
        /// </summary>
        public IDictionary<string, object>? Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStreamRequest"/> class.
        /// </summary>
        /// <param name="sql">The SELECT statement.</param>
        /// <param name="properties">The properties.</param>
        public QueryStreamRequest(string sql, IDictionary<string, object>? properties)
        {
            Sql = !string.IsNullOrWhiteSpace(sql) ? sql : throw new ArgumentNullException(nameof(sql));
            Properties = properties;
        }
    }
}
