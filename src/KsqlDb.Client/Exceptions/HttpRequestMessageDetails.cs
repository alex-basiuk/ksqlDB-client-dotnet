using System;
using System.Collections.Generic;
using System.Net.Http;

namespace KsqlDb.Api.Client.Exceptions
{
    /// <summary>
    /// The details of the http request.
    /// </summary>
    /// <remarks>
    /// It copies data from the respective <see cref="HttpRequestMessage"/> and not linked to the lifetime of the respective <see cref="HttpClient"/>.
    /// </remarks>
    public class HttpRequestMessageDetails : HttpMessageDetails
    {
        /// <summary>
        /// The Http method used by the Http request message.
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// The Uri used for the Http request.
        /// </summary>
        public Uri? RequestUri { get; }

        /// <summary>
        /// The properties for the Http request.
        /// </summary>
        public IDictionary<string, object?> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessageDetails"/> class.
        /// </summary>
        /// <param name="httpRequest">The http request message.</param>
        /// <param name="content">The http message content.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="httpRequest"/> is not provided.</exception>
        public HttpRequestMessageDetails(HttpRequestMessage httpRequest, string? content)
            : base(content)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            CopyHeaders(httpRequest.Headers, httpRequest.Content?.Headers);
            Method = httpRequest.Method;
            RequestUri = httpRequest.RequestUri;
#if NET5_0
            Properties = new Dictionary<string, object?>(httpRequest.Options);
#else
            Properties = new Dictionary<string, object?>(httpRequest.Properties);
#endif
        }
    }
}
