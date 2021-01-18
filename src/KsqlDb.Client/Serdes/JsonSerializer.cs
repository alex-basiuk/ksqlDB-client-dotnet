using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serializer = System.Text.Json.JsonSerializer;

namespace KsqlDb.Api.Client.Serdes
{
    /// <summary>
    /// The JSON serializer for ksqlDB requests and responses.
    /// </summary>
    public class JsonSerializer : IJsonSerializer
    {
        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = false,
            IgnoreNullValues = true,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            WriteIndented = false,
            Converters =
            {
                new KSqlObjectConverter(),
                new KSqlArrayConverter(),
                new KSqlNullConverter()
            }
        };

        /// <inheritdoc />
        public string Serialize<TValue>(TValue value) => Serializer.Serialize(value, _serializerOptions);

        /// <inheritdoc />
        public Task SerializeAsync(Stream utf8JsonStream, object value, CancellationToken cancellationToken = default) =>
            Serializer.SerializeAsync(utf8JsonStream, value, value.GetType(), _serializerOptions, CancellationToken.None);

        /// <inheritdoc />
        public async ValueTask<TValue> DeserializeAsync<TValue>(Stream utf8JsonStream, CancellationToken cancellationToken = default)
        {
            var value = await Serializer.DeserializeAsync<TValue>(utf8JsonStream, _serializerOptions, cancellationToken);
            return value ?? throw new JsonException($"Unable to deserialize the provided UTF8 JSON stream into {typeof(TValue).FullName}");
        }

        /// <inheritdoc />
        public TValue Deserialize<TValue>(string json)
        {
            var value = Serializer.Deserialize<TValue>(json, _serializerOptions);
            return value ?? throw new JsonException($"Unable to deserialize the following JSON into {typeof(TValue).FullName}: {json}");
        }

        /// <inheritdoc />
        public TValue? TryDeserialize<TValue>(string? json) where TValue : class
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            try
            {
                return Deserialize<TValue>(json);
            }
            catch (JsonException)
            {
                return default;
            }
        }
    }
}
