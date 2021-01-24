using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Exceptions;
using Xunit;

namespace KsqlDb.Client.IntegrationTests
{
    public class StreamInsertsTests
    {
        private const int RowsNumber = 100;
        private const string PrimitiveStreamName = "PrimitiveStream";

        [Fact]
        public async Task Inserts_Rows_With_Primitive_Type_Fields()
        {
            // Arrange
            await CreatePrimitiveStream();
            int[] rowIndices = Enumerable.Range(1, RowsNumber).ToArray();
            var expectedSequenceNumbers = rowIndices.Select(x => (long)x-1);
            var dataToInsert = rowIndices.Select(CreatePrimitiveRow).ToArray();
            var expectedRows = dataToInsert.Select(GetKsqlObjectValuesSortedByFieldName).ToArray();
            var asyncRows = dataToInsert.ToAsyncEnumerable();

            // Act
            var acks = TestClient.Instance.StreamInserts(PrimitiveStreamName, asyncRows);

            // Assert
            var actualSequenceNumbers = (await acks.ToArrayAsync()).Select(x => x.SequenceNumber);
            Assert.Equal(expectedSequenceNumbers, actualSequenceNumbers);
            var actualRows = await QueryStream(PrimitiveStreamName, typeof(int), typeof(long), typeof(bool), typeof(double), typeof(string));
            Assert.Equal(expectedRows, actualRows, new TwoDimensionalArrayEqualityComparer());
        }

        private static KSqlObject CreatePrimitiveRow(int rowNumber) =>
            new KSqlObject()
                .Add("f1", rowNumber * 10)
                .Add("f2", rowNumber * 10_000L)
                .Add("f3", rowNumber % 2 == 0)
                .Add("f4", 10007d / rowNumber)
                .Add("f5", $"{rowNumber} row");

        private static async Task CreatePrimitiveStream()
        {
            try
            {
                // Drop the stream if already exists
                await TestClient.Instance.ExecuteStatement($"DROP STREAM {PrimitiveStreamName};");
            }
            catch (KsqlDbException)
            {
                // The stream doesn't exist
            }

            await TestClient.Instance.ExecuteStatement(
               @$"CREATE STREAM {PrimitiveStreamName} (
                        f1 INTEGER KEY,
                        f2 BIGINT,
                        f3 BOOLEAN,
                        f4 DOUBLE,
                        f5 VARCHAR
                    ) WITH (
                        kafka_topic = '{PrimitiveStreamName}',
                        partitions = 1,
                        value_format = 'json'
                    );");
        }

        private static async Task<object[][]> QueryStream(string streamName, params Type[] expectedTypes)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/query-stream")
            {
                Content = new StringContent($"{{ \"sql\": \"select * from {streamName} emit changes limit {RowsNumber};\", \"properties\": {{ \"ksql.streams.auto.offset.reset\": \"earliest\" }} }}", Encoding.UTF8, "application/json"),
                Version = new Version(2, 0),
#if NET5_0
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
#endif
            };

            var response = await TestClient.RawHttpClient.SendAsync(request);
            Debug.Assert(response.IsSuccessStatusCode);
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(responseStream);
            _ = await streamReader.ReadLineAsync(); // skip data schema header
            var rows = new List<object[]>();
            while (!streamReader.EndOfStream)
            {
                string responseContentLine = await streamReader.ReadLineAsync();
                var parsedRow = JsonSerializer.Deserialize<object[]>(responseContentLine!);
                var processedRow = parsedRow!.Cast<JsonElement>().Select((e, i) => JsonSerializer.Deserialize(e.GetRawText(), expectedTypes[i])).ToArray();
                rows.Add(processedRow);
            }

            return rows.ToArray();
        }

        private static object[] GetKsqlObjectValuesSortedByFieldName(KSqlObject kSqlObject) =>
            kSqlObject.AsImmutableDictionary().OrderBy(x => x.Key).Select(x => x.Value).ToArray();

        private sealed class TwoDimensionalArrayEqualityComparer : IEqualityComparer<object[][]>
        {
            public bool Equals(object[][] x, object[][] y) => x!.SequenceEqual(y!, new ArrayEqualityComparer());
            public int GetHashCode(object[][] obj) => 0;
        }

        private sealed class ArrayEqualityComparer : IEqualityComparer<object[]>
        {
            public bool Equals(object[] x, object[] y)
            {
                if (x!.Length != y!.Length) return false;
                for (int i = 0; i < x.Length; i++)
                {
                    if (!x[i].Equals(y[i])) return false;
                }

                return true;
            }
            public int GetHashCode(object[] obj) => 0;
        }
    }
}
