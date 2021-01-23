using System;
using System.Linq;
using System.Text.Json;
using KsqlDb.Api.Client.Parsers;
using Xunit;

namespace KsqlDb.Client.UnitTests
{
    public class QueryResultRowParserTests
    {
        [Fact]
        public void Pr()
        {
            // Arrange
            const string row = @"{
                                 ""columnNames"":[""USERID"",""REGISTERTIME"",""REGIONID"",""GENDER"",""INTERESTS"",""CONTACTINFO""],
                                 ""columnTypes"":[""STRING"",""BIGINT"",""STRING"",""STRING"",""ARRAY<STRING>"",""MAP<STRING, STRING>""]
                                 }";
           var rowJson = (JsonElement)JsonSerializer.Deserialize<object>(row, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

           var target = new QueryResultRowParser(rowJson);

        }
    }
}
