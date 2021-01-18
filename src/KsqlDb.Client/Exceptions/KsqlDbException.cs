using System;

namespace KsqlDb.Api.Client.Exceptions
{
    /// <summary>
    /// An exception generated from an http response returned bt the ksqlDB.
    /// </summary>
    public class KsqlDbException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KsqlDbException"/> class.
        /// </summary>
        public KsqlDbException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KsqlDbException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public KsqlDbException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KsqlDbException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public KsqlDbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets information about the associated HTTP request.
        /// </summary>
        public HttpRequestMessageDetails? Request { get; set; }

        /// <summary>
        /// Gets information about the associated HTTP response.
        /// </summary>
        public HttpResponseMessageDetails? Response { get; set; }

        /// <summary>
        /// Gets or sets the response object.
        /// </summary>
        public ErrorDetails? Body { get; set; }
    }
}
