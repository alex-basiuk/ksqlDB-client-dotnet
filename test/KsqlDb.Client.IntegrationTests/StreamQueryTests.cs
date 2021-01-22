using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Abstractions.QueryResults;
using KsqlDb.Api.Client.Exceptions;
using Xunit;

namespace KsqlDb.Client.IntegrationTests
{
    public class StreamQueryTests : IClassFixture<StreamQueryTests.StreamQueryFixture>
    {
        private const int ExpectedRowsCount = 3;
        private readonly IReadOnlyCollection<(string name, Type type)> _actualColumns;
        private readonly QueryResultRow[] _actualRows;
        private readonly string _actualQueryId;

        public StreamQueryTests(StreamQueryFixture fixture) => (_actualColumns, _actualRows, _actualQueryId) = (fixture.Columns, fixture.Rows, fixture.QueryId);

        [Fact]
        public void Response_Contains_Expected_Number_Of_Result_Rows()
        {
            Assert.Equal(ExpectedRowsCount, _actualRows.Length);
        }

        [Fact]
        public void Response_Contains_USERID_Values_In_Expected_Format_Column()
        {
            string[] actual = _actualRows.Select(r => r.AsArray.GetString(0)).ToArray();
            Assert.All(actual, u => Assert.StartsWith("User_", u));
        }

        [Fact]
        public void Response_Contains_REGISTERTIME_Values_In_Expected_Format_Column()
        {
            long[] actual = _actualRows.Select(r => r.AsArray.GetLong(1)).ToArray();
            Assert.All(actual, t => Assert.InRange(t, 0L, long.MaxValue));
        }

        [Fact]
        public void Response_Contains_REGIONID_Values_In_Expected_Format_Column()
        {
            string[] actual = _actualRows.Select(r => r.AsArray.GetString(3)).ToArray();
            Assert.All(actual, u => Assert.StartsWith("Region_", u));
        }

        [Fact]
        public void Response_Contains_GENDER_Values_In_Expected_Format_Column()
        {
            string[] actual = _actualRows.Select(r => r.AsArray.GetString(2)).ToArray();
            Assert.All(actual, g => Assert.Matches("MALE|FEMALE|OTHER", g));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        public void Response_Contains_INTEREST_Values_In_Expected_Format_Column(int row, int arrayIndex)
        {
            string actual = _actualRows[row].GetKSqlArray("INTERESTS").GetString(arrayIndex);
            Assert.Matches("News|Travel|Movies|Sport|Game|Travel", actual);
        }

        [Fact]
        public void Response_Contains_Different_USERID_Values()
        {
            int uniqueValues = _actualRows.Select(r => r.AsArray.GetString(0)).Distinct().Count();
            Assert.True(uniqueValues > 0);
        }

        [Fact]
        public void Response_Contains_Expected_Column_Definitions()
        {
            Assert.Collection(_actualColumns,
                              column => Assert.True(column.name == "USERID" && column.type == typeof(string), "USERID"),
                              column => Assert.True(column.name == "REGISTERTIME" && column.type == typeof(long), "REGISTERTIME"),
                              column => Assert.True(column.name == "GENDER" && column.type == typeof(string), "GENDER"),
                              column => Assert.True(column.name == "REGIONID" && column.type == typeof(string), "REGIONID"),
                              column => Assert.True(column.name == "INTERESTS" && column.type == typeof(KSqlArray), "INTERESTS"),
                              column => Assert.True(column.name == "CONTACTINFO" && column.type == typeof(KSqlObject), "CONTACTINFO"));
        }

        [Fact]
        public void Response_Contains_Non_Empty_QueryId()
        {
            Assert.NotEmpty(_actualQueryId);
        }

        [Fact]
        public async Task Throws_When_Supplied_Invalid_Sql()
        {
            var exception = await Assert.ThrowsAsync<KsqlDbException>(() => TestClient.Instance.StreamQuery("Invalid sql;"));
            Assert.NotNull(exception.Body);
            Assert.Equal(40001, exception.Body.ErrorCode);
        }

        [Fact]
        public async Task Throws_When_Queries_Non_Existing_Stream()
        {
            var exception = await Assert.ThrowsAsync<KsqlDbException>(() => TestClient.Instance.StreamQuery("SELECT * FROM NONEXISTINGSTREAM EMIT CHANGES LIMIT 1;"));
            Assert.NotNull(exception.Body);
            Assert.Equal(40001, exception.Body.ErrorCode);
        }

        [Collection(nameof(StreamQueryFixture))]
        public sealed class StreamQueryFixture : IAsyncLifetime
        {
            public IReadOnlyCollection<(string name, Type type)> Columns { get; set; }
            public QueryResultRow[] Rows { get; set; }
            public string QueryId { get; set; }

            public async Task InitializeAsync()
            {
                var properties = new Dictionary<string, object> { ["ksql.streams.auto.offset.reset"] = "earliest" };
                try
                {
                    var queryResult = await TestClient.Instance.StreamQuery($"SELECT * FROM {TestClient.KnownEntities.UsersTableName} EMIT CHANGES LIMIT {ExpectedRowsCount};", properties);
                    Columns = queryResult.Columns;
                    QueryId = queryResult.QueryId;
                    Rows = await queryResult.Rows.ToArrayAsync();
                }
                catch (KsqlDbException e)
                {
                    throw new Exception($"Error code: {e.Body?.ErrorCode ?? -1}, Error message: {e.Body?.Message ?? ""}", e);
                }
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }
    }
}
