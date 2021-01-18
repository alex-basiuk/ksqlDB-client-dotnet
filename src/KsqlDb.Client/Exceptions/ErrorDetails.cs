using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KsqlDb.Api.Client.Exceptions
{
    /// <summary>
    /// ksqlDB error details.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// The error code.
        /// </summary>
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
#nullable disable annotations
        public string Message { get; set; }
#nullable restore annotations

        /// <summary>
        /// The additional fields returned by some endpoint.
        /// They provide more context for handling the error.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalFields { get; set; }
    }
}
