using System;
using System.Linq;
using System.Threading.Tasks;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Exceptions;
using KsqlDb.Api.Client.KsqlApiV1;
using Xunit;
using Xunit.Abstractions;

namespace KsqlDb.Client.IntegrationTests
{
    public class StreamInsertsTests
    {
        private readonly TestClient _client;
        private const string PrimitiveStreamName = "PrimitiveStream";

        public StreamInsertsTests(ITestOutputHelper output)
        {
            _client = new TestClient(output);
        }

        [Fact]
        public async Task Inserts_Rows_With_Primitive_Type()
        {
            // Arrange
            await CreatePrimitiveStream();
            var rowIndices = Enumerable.Range(1, 100).ToArray();
            var expected = rowIndices.Select(x => (long)x-1);
            var rows = rowIndices.Select(CreatePrimitiveRow).ToAsyncEnumerable();

            // Act
            var acks = TestClient.Instance.StreamInserts(PrimitiveStreamName, rows);

            // Assert
            var actual = (await acks.ToArrayAsync()).Select(x => x.SequenceNumber);

            Assert.Equal(expected, actual);
        }

        private static KSqlObject CreatePrimitiveRow(int rowNumber) =>
            new KSqlObject()
                .Add("i1", rowNumber * 10)
                .Add("b2", rowNumber * 10_000)
                .Add("b3", rowNumber % 2 == 0)
                .Add("d4", 10007d / rowNumber)
                .Add("v5", $"{rowNumber} row");

        private static async Task CreatePrimitiveStream()
        {
            try
            {
                await TestClient.Instance.ExecuteStatement($"DROP STREAM {PrimitiveStreamName};");
            }
            catch (KsqlDbException)
            {

            }

            await TestClient.Instance.ExecuteStatement(
               @$"CREATE STREAM {PrimitiveStreamName} (
                        i1 INTEGER KEY,
                        b2 BIGINT,
                        b3 BOOLEAN,
                        d4 DOUBLE,
                        v5 VARCHAR
                    ) WITH (
                        kafka_topic = '{PrimitiveStreamName}',
                        partitions = 1,
                        value_format = 'json'
                    );");
        }
    }
}
