using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KsqlDb.Api.Client.Serdes
{
    /// <summary>
    /// The JSON serializer abstraction.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Convert the provided value to UTF-8 encoded JSON text and write it to the <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <param name="utf8JsonStream">The UTF-8 <see cref="System.IO.Stream"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the write operation.</param>
        Task SerializeAsync(Stream utf8JsonStream, object value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Convert the provided value into a JSON <see cref="System.String"/>.
        /// </summary>
        /// <returns>A JSON <see cref="System.String"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        string Serialize<TValue>(TValue value);

        /// <summary>
        /// Read the UTF-8 encoded text representing a single JSON value into a <typeparamref name="TValue"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        /// <param name="utf8JsonStream">JSON data to parse.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the read operation.
        /// </param>
        /// <exception cref="JsonException">
        /// Thrown when the JSON is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the JSON,
        /// or when there is remaining data in the Stream.
        /// </exception>
        ValueTask<TValue> DeserializeAsync<TValue>(Stream utf8JsonStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Parse the text representing a single JSON value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="json"/> is null.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown when the JSON is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the JSON,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <remarks>Using a <see cref="System.String"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        TValue Deserialize<TValue>(string json);

        /// <summary>
        /// Attempts to parse the text representing a single JSON value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="json">JSON text to parse.</param>
        /// <typeparam name="TValue">The result type.</typeparam>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        TValue? TryDeserialize<TValue>(string? json) where TValue : class;
    }
}
