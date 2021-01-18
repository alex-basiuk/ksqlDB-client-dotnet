using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KsqlDb.Api.Client.KsqlApiV1.Requests;
using KsqlDb.Api.Client.Serdes;

namespace KsqlDb.Api.Client.KsqlApiV1.Content
{
    /// <summary>
    /// The factory for the V1 request schema.
    /// </summary>
    internal class KsqlV1RequestHttpContentFactory : IKsqlRequestHttpContentFactory
    {
        private static readonly MediaTypeHeaderValue s_ksqlV1 = new MediaTypeHeaderValue(KSqlDbHttpClient.SupportedMediaType);
        private static readonly MediaTypeHeaderValue s_ksqlDelimitedV1 = new MediaTypeHeaderValue(KSqlDbHttpClient.SupportedDelimitedMediaType);

        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="KsqlV1RequestHttpContentFactory"/> class.
        /// </summary>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        public KsqlV1RequestHttpContentFactory(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));

        /// <inheritdoc />
        public HttpContent CreateContent(object value) => new KsqlV1HttpContent(_jsonSerializer, value, s_ksqlV1);

        /// <inheritdoc />
        public HttpContent CreateDelimitedContent(object value) => new KsqlV1HttpContent(_jsonSerializer, value, s_ksqlDelimitedV1);

        /// <inheritdoc />
        public HttpContent CreateDelimitedStreamingContent<T>(string target, IAsyncEnumerable<T> values) where T : class
        {
            /*async Task ToStream(Stream stream)
            {
                await _jsonSerializer.SerializeAsync(stream, new TargetStream {Target = target});
                await _jsonSerializer.SerializeAsync(stream, Environment.NewLine);
                await foreach (T value in values.ConfigureAwait(false))
                {
                    await _jsonSerializer.SerializeAsync(stream, value);
                    await _jsonSerializer.SerializeAsync(stream, Environment.NewLine);
                }
            }

            using var stream = new MemoryStream();
            ToStream(stream);
            var sc = new StreamContent();
            sc.Headers.ContentType = s_ksqlDelimitedV1;*/
            return new KsqlV1StreamingHttpContent<T>(_jsonSerializer, new TargetStream {Target = target}, values, s_ksqlDelimitedV1);
        }

        private sealed class KsqlV1HttpContent : HttpContent
        {
            private readonly IJsonSerializer _jsonSerializer;
            private readonly object _value;

            public KsqlV1HttpContent(IJsonSerializer jsonSerializer, object value, MediaTypeHeaderValue supportedMediaTypeHeader)
            {
                _jsonSerializer = jsonSerializer;
                _value = value;
                Headers.ContentType = supportedMediaTypeHeader;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => _jsonSerializer.SerializeAsync(stream, _value);

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        private sealed class KsqlV1StreamingHttpContent<T> : HttpContent where T : class
        {
            private readonly IJsonSerializer _jsonSerializer;
            private readonly TargetStream _target;
            private readonly IAsyncEnumerable<T> _values;

            public KsqlV1StreamingHttpContent(IJsonSerializer jsonSerializer, TargetStream target, IAsyncEnumerable<T> values, MediaTypeHeaderValue supportedMediaTypeHeader)
            {
                _jsonSerializer = jsonSerializer;
                _target = target;
                _values = values;
                Headers.ContentType = supportedMediaTypeHeader;
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                byte[] lineSeparator = Encoding.UTF8.GetBytes(Environment.NewLine);
                await _jsonSerializer.SerializeAsync(stream, _target);
                await stream.WriteAsync(lineSeparator);
                await foreach (T value in _values.ConfigureAwait(false))
                {
                    await _jsonSerializer.SerializeAsync(stream, value);
                    await stream.WriteAsync(lineSeparator);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }
    }
}
