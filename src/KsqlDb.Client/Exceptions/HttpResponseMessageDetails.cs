using System.Net;
using System.Net.Http;

namespace KsqlDb.Api.Client.Exceptions
{
    /// <summary>
    /// The details of the http response.
    /// </summary>
    /// <remarks>
    /// It copies data from the respective <see cref="HttpResponseMessage"/> and not linked to the lifetime of the respective <see cref="HttpClient"/>.
    /// </remarks>
    public class HttpResponseMessageDetails : HttpMessageDetails
    {
        /// <summary>
        /// The status code of the Http response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The reason phrase.
        /// </summary>
        /// <remarks>
        /// It typically sent along with the status code.
        /// </remarks>
        public string? ReasonPhrase { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMessageDetails"/> class.
        /// </summary>
        /// <param name="httpResponse">The http response message.</param>
        /// <param name="content">The http message content.</param>
        public HttpResponseMessageDetails(HttpResponseMessage? httpResponse, string? content = null)
            : base(content)
        {
            if (httpResponse == null) return;
            CopyHeaders(httpResponse.Headers, httpResponse.Content.Headers);
            StatusCode = httpResponse.StatusCode;
            ReasonPhrase = httpResponse.ReasonPhrase;
        }
    }
}
