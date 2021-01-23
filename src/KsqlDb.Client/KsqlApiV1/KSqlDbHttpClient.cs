using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Exceptions;
using KsqlDb.Api.Client.KsqlApiV1.Content;
using KsqlDb.Api.Client.KsqlApiV1.Requests;
using KsqlDb.Api.Client.KsqlApiV1.Responses;
using KsqlDb.Api.Client.Serdes;

namespace KsqlDb.Api.Client.KsqlApiV1
{
    internal class KSqlDbHttpClient
    {
        internal const string SupportedMediaType = "application/vnd.ksql.v1+json";
        internal const string SupportedDelimitedMediaType = "application/vnd.ksqlapi.delimited.v1";

        private readonly IKsqlRequestHttpContentFactory _httpContentFactory;
        private readonly IJsonSerializer _jsonSerializer;

        private static class Endpoints
        {
            public const string QueryStream = "/query-stream";
            public const string CloseStream = "/close-stream";
            public const string InsertStream = "/inserts-stream";
            public const string Ksql = "/ksql";
        }

        private readonly HttpClient _httpClient;

        public KSqlDbHttpClient(HttpClient httpClient, IKsqlRequestHttpContentFactory httpContentFactory, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContentFactory = httpContentFactory ?? throw new ArgumentNullException(nameof(httpContentFactory));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            //if (_httpClient.BaseAddress.Scheme != Uri.UriSchemeHttps) AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SupportedMediaType));
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(SupportedDelimitedMediaType));
        }

        public async Task PostQueryStream(ChannelWriter<JsonElement> channelWriter, string sql, IDictionary<string, object>? properties = null, CancellationToken cancellationToken = default)
        {
            var requestContent = new QueryStreamRequest(sql, properties);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoints.QueryStream)
            {
                Content = _httpContentFactory.CreateDelimitedContent(requestContent),
                Version = new Version(2, 0),
#if NET5_0
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
#endif
            };

            HttpResponseMessage response = null!;
            try
            {
                response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                //EnsureResponseMediaType(response, SupportedDelimitedMediaType); // The header is not available
                await using var responseStream = await ReadAsStream(response, cancellationToken);
                using var sr = new StreamReader(responseStream, Encoding.UTF8);
                while (!sr.EndOfStream)
                {
                    string? line = await sr.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    //TODO It's convenient to see string representation while developing the component, but it's suboptimal.
                    var deserializedLine = (JsonElement)_jsonSerializer.Deserialize<object>(line);
                    await channelWriter.WriteAsync(deserializedLine, cancellationToken);
                }

                channelWriter.Complete();
            }
            catch (HttpRequestException e)
            {
                string? responseContent = await ReadAsString(response, cancellationToken);
                channelWriter.Complete(new KsqlDbException($"An error occured while requesting {httpRequestMessage.RequestUri} endpoint.", e)
                {
                    Request = new HttpRequestMessageDetails(httpRequestMessage, _jsonSerializer.Serialize(requestContent)),
                    Response = new HttpResponseMessageDetails(response, responseContent),
                    Body = _jsonSerializer.TryDeserialize<ErrorDetails>(responseContent),

                });
            }
            catch (JsonException e)
            {
                channelWriter.Complete(new KsqlDbException($"Unable to deserialize JSON returned by {httpRequestMessage.RequestUri} endpoint.", e)
                {
                    Request = new HttpRequestMessageDetails(httpRequestMessage, _jsonSerializer.Serialize(requestContent)),
                    Response = new HttpResponseMessageDetails(response)
                });
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task PostInsertStream(string streamName, IAsyncEnumerable<KSqlObject> rows, ChannelWriter<InsertStreamAckResponse> ackChannelWriter, CancellationToken cancellationToken = default)
        {
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoints.InsertStream)
            {
                Content = _httpContentFactory.CreateDelimitedStreamingContent(streamName, rows),
                Version = new Version(2, 0),
#if NET5_0
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
#endif
            };

            HttpResponseMessage response = null!;
            try
            {
                response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                //EnsureResponseMediaType(response, SupportedDelimitedMediaType); // The header is not available
                await using var responseStream = await ReadAsStream(response, cancellationToken);
                using var sr = new StreamReader(responseStream, Encoding.UTF8);
                while (!sr.EndOfStream)
                {
                    string? line = await sr.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    //TODO It's convenient to see string representation while developing the component, but it's suboptimal.
                    var deserializedLine = _jsonSerializer.Deserialize<InsertStreamAckResponse>(line);
                    await ackChannelWriter.WriteAsync(deserializedLine, cancellationToken);
                }

                ackChannelWriter.Complete();
            }
            catch (HttpRequestException e)
            {
                string? responseContent = await ReadAsString(response, cancellationToken);
                ackChannelWriter.Complete(new KsqlDbException($"An error occured while requesting {httpRequestMessage.RequestUri} endpoint.", e)
                {
                    Request = new HttpRequestMessageDetails(httpRequestMessage, null),
                    Response = new HttpResponseMessageDetails(response, responseContent),
                    Body = _jsonSerializer.TryDeserialize<ErrorDetails>(responseContent),

                });
            }
            catch (JsonException e)
            {
                ackChannelWriter.Complete(new KsqlDbException($"Unable to deserialize JSON returned by {httpRequestMessage.RequestUri} endpoint.", e)
                {
                    Request = new HttpRequestMessageDetails(httpRequestMessage, null),
                    Response = new HttpResponseMessageDetails(response)
                });
            }
            finally
            {
                response.Dispose();
            }
        }

        public async Task PostCloseStream(string queryId, CancellationToken cancellationToken = default)
        {
            var request = new CloseStreamRequest(queryId);
            var response = await _httpClient.PostAsync(Endpoints.CloseStream, _httpContentFactory.CreateContent(request), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<KsqlResponse[]> PostKqsl(string sql, IDictionary<string, object>? properties = null, long? commandSequenceNumber = null, CancellationToken cancellationToken = default)
        {
            var requestContent = new KsqlRequest(sql, properties, commandSequenceNumber);

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoints.Ksql)
            {
                Content = _httpContentFactory.CreateContent(requestContent),
                Version = new Version(2, 0),
#if NET5_0
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
#endif
            };
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(SupportedMediaType));

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                EnsureResponseMediaType(response, "application/json"); //TODO Review?
                var responseStream = await ReadAsStream(response, cancellationToken);
                return await _jsonSerializer.DeserializeAsync<KsqlResponse[]>(responseStream, cancellationToken);
            }
            catch (HttpRequestException e)
            {
                string? responseContent = await ReadAsString(response, cancellationToken);
                throw new KsqlDbException($"An error occurred while requesting {httpRequestMessage.RequestUri} endpoint.", e)
                {
                    Request = new HttpRequestMessageDetails(httpRequestMessage, _jsonSerializer.Serialize(requestContent)),
                    Response = new HttpResponseMessageDetails(response, responseContent),
                    Body = _jsonSerializer.TryDeserialize<ErrorDetails>(responseContent)
                };
            }
            catch (JsonException e)
            {
                string? responseContent = await ReadAsString(response, cancellationToken);
                throw new KsqlDbException($"Unable to deserialize JSON \"{responseContent ?? string.Empty}\" returned by {httpRequestMessage.RequestUri} endpoint.", e);
            }
        }

        private static async Task<string?> ReadAsString(HttpResponseMessage? responseMessage, CancellationToken cancellationToken)
        {
            if (responseMessage is null) return null;
#if NET5_0
            return await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        private static async Task<Stream> ReadAsStream(HttpResponseMessage? responseMessage, CancellationToken cancellationToken)
        {
            if (responseMessage is null) return Stream.Null;
#if NET5_0
            return await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            return await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
        }

        private static void EnsureResponseMediaType(HttpResponseMessage response, string expectedMediaType)
        {
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, expectedMediaType, StringComparison.Ordinal))
            {
                throw new HttpRequestException($"The response media type \"{response.Content.Headers.ContentType?.MediaType ?? string.Empty}\" is not supported.");
            }
        }
    }
}
