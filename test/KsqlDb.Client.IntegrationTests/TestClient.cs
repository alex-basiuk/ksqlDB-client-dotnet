using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KsqlDb.Api.Client.KsqlApiV1;
using KsqlDb.Api.Client.KsqlApiV1.Content;
using KsqlDb.Api.Client.Serdes;
using Xunit.Abstractions;

namespace KsqlDb.Client.IntegrationTests
{
    internal class TestClient : KsqlDb.Api.Client.Client
    {
        public static class KnownEntities
        {
            public const string OrdersTopicName = "orders_topic";
            public const string OrdersStreamName = "ORDERS_STREAM";
            public const string UsersTopicName = "users_topic";
            public const string UsersTableName = "USERS_TABLE";
        }

        public static KsqlDb.Api.Client.Client Instance => new TestClient();

        private TestClient() : base(CreateHttpClient())
        {
        }

        private static KSqlDbHttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient {BaseAddress = new Uri("http://127.0.0.1:8088")};
            var jsonSerializer = new JsonSerializer();
            return new KSqlDbHttpClient(httpClient, new KsqlV1RequestHttpContentFactory(jsonSerializer), jsonSerializer);
        }

        public TestClient(ITestOutputHelper output) : base(CreateHttpClientWithLogging(output))
        {

        }

        public static KSqlDbHttpClient CreateHttpClientWithLogging(ITestOutputHelper output)
        {
            var httpClient = new HttpClient(new LoggingHandler(output)) {BaseAddress = new Uri("http://127.0.0.1:8088")};
            var jsonSerializer = new JsonSerializer();
            return new KSqlDbHttpClient(httpClient, new KsqlV1RequestHttpContentFactory(jsonSerializer), jsonSerializer);
        }
    }

    internal class LoggingHandler : DelegatingHandler
    {
        private readonly ITestOutputHelper _output;

        public LoggingHandler(ITestOutputHelper output)
        {
            _output = output;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _output.WriteLine(request.ToString());
            if (request.Content is not null)
            {
                var content = await request.Content.ReadAsStringAsync();
                _output.WriteLine($"Content: {content}");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
