using System;
using KsqlDb.Api.Client.Abstractions;
using Xunit;

namespace KsqlDb.Client.IntegrationTests
{
    public class DescribeTests : IClassFixture<DescribeTests.SourceDescriptionsFixture>
    {
        private readonly SourceDescriptionsFixture _fixture;

        public DescribeTests(SourceDescriptionsFixture fixture) => _fixture = fixture;

        [Theory]
        [InlineData(TestClient.KnownEntities.OrdersStreamName)]
        [InlineData(TestClient.KnownEntities.UsersTableName)]
        public void Contains_Statement_Text(string entity)
        {
            Assert.Contains($"DESCRIBE {entity}",_fixture.OrdersStreamDescription.SqlStatement);
        }

        /*[Theory]

        public void Orders_Stream_Contains_ExpectedFields()
        {

        }*/

        public sealed class SourceDescriptionsFixture : IDisposable
        {
            public SourceDescription OrdersStreamDescription { get; }
            public SourceDescription UsersTableDescription { get; }

            public SourceDescriptionsFixture()
            {
                OrdersStreamDescription = TestClient.Instance.DescribeSource(TestClient.KnownEntities.OrdersStreamName).GetAwaiter().GetResult();
                UsersTableDescription = TestClient.Instance.DescribeSource(TestClient.KnownEntities.UsersTableName).GetAwaiter().GetResult();
            }

            public void Dispose()
            {
            }
        }
    }
}
