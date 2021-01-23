using System.Text.Json.Serialization;

namespace KsqlDb.Api.Client.KsqlApiV1.Responses
{
    internal class InsertStreamAckResponse
    {
#nullable disable annotations
        public string Status { get; set; }
#nullable restore annotations

        public long Seq { get; set; }

        /// <summary>
        /// The error code.
        /// </summary>
        [JsonPropertyName("error_code")]
        public int? ErrorCode { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string? Message { get; set; }
    }
}
