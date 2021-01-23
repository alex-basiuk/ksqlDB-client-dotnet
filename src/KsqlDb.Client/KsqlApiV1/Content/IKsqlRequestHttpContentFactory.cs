using System.Collections.Generic;
using System.Net.Http;

namespace KsqlDb.Api.Client.KsqlApiV1.Content
{
    /// <summary>
    /// The ksqlDb request content factory.
    /// </summary>
    internal interface IKsqlRequestHttpContentFactory
    {
        /// <summary>
        /// Create an instance of ksqDb request content.
        /// </summary>
        /// <param name="value">The request.</param>
        /// <returns>The instance of <see cref="HttpContent"/>.</returns>
        HttpContent CreateContent(object value);

        /// <summary>
        /// Create an instance of ksqDb delimited request content.
        /// </summary>
        /// <param name="value">The request.</param>
        /// <returns>The instance of <see cref="HttpContent"/>.</returns>
        HttpContent CreateDelimitedContent(object value);

        public HttpContent CreateDelimitedStreamingContent<T>(string target, IAsyncEnumerable<T> values) where T : class;
    }
}
