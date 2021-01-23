using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace KsqlDb.Api.Client.Exceptions
{
    /// <summary>
    /// The base class for <see cref="HttpRequestMessageDetails"/> and <see cref="HttpResponseMessageDetails"/>.
    /// It contains request/response details which are not linked to the lifetime of the respective <see cref="HttpClient"/>.
    /// </summary>
    public class HttpMessageDetails
    {
        /// <summary>
        /// The Http message contents.
        /// </summary>
        public string? Content { get; }

        /// <summary>
        /// The Http headers.
        /// </summary>
        public IDictionary<string, IEnumerable<string>> Headers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageDetails"/> class.
        /// </summary>
        /// <param name="content">The http message content.</param>
        protected HttpMessageDetails(string? content)
        {
            Content = content;
            Headers = new Dictionary<string, IEnumerable<string>>();
        }

        /// <summary>
        /// Copies Http headers
        /// </summary>
        /// <param name="messageHeaders">The message headers.</param>
        /// <param name="contentHeaders">The content headers.</param>
        protected void CopyHeaders(HttpHeaders? messageHeaders, HttpHeaders? contentHeaders)
        {
            static IEnumerable<T> EmptyIfNull<T>(IEnumerable<T>? e) => e ?? Enumerable.Empty<T>();

            var headers = EmptyIfNull(messageHeaders).Concat(EmptyIfNull(contentHeaders));

            foreach (var (key, value) in headers)
            {
                if (Headers.TryGetValue(key, out var existingValues))
                {
                    existingValues = existingValues.Concat(value).ToArray();
                }
                else
                {
                    existingValues = value;
                }
                Headers[key] = existingValues;
            }
        }
    }
}
