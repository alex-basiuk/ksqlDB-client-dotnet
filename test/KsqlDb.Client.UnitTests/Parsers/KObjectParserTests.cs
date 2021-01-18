using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KsqlDb.Api.Client.Abstractions.Objects;
using KsqlDb.Api.Client.Parsers;
using Xunit;

namespace KsqlDb.Client.UnitTests.Parsers
{
    public class KObjectParserTests
    {
        [Theory]
        [InlineData("STRING")]
        [InlineData("String")]
        [InlineData("INTEGER")]
        [InlineData("BIGINT")]
        [InlineData("DOUBLE")]
        [InlineData("BOOLEAN")]
        [InlineData("DECIMAL")]
        [InlineData("ARRAY<INTEGER>")]
        [InlineData("MAP<STRING, INTEGER>")]
        [InlineData("ARRAY<MAP<STRING, ARRAY<DECIMAL>>>")]
        [InlineData("STRUCT<ID BIGINT, NAMES ARRAY<STRING>, AGE INT>")]
        public void Creates_A_Parser_For_A_Known_Type(string type)
        {
            var target = KObjectParser.Create(type);
            Assert.NotNull(target);
        }

        [Fact]
        public void Throws_NotSupportedException_When_Created_For_An_Unknown_Type()
        {
            Assert.Throws<NotSupportedException>(() => KObjectParser.Create("HASHMAP<STRING, INT>"));
        }

        [Theory]
        [InlineData("INTEGER", typeof(int))]
        [InlineData("BIGINT", typeof(long))]
        [InlineData("DOUBLE", typeof(double))]
        [InlineData("DECIMAL", typeof(decimal))]
        public void Parses_A_Numeric_Json_Value_And_Returns_An_Instance_Of_The_Expected_Type(string ksqlType, Type expectedType)
        {
            // Arrange
            const int value = 42;
            var expected = Convert.ChangeType(value, expectedType);
            var jsonElement = Deserialize(value.ToString());
            var target = KObjectParser.Create(ksqlType);

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("INTEGER", "42.42")]
        [InlineData("BIGINT", "42.42")]
        public void Parses_A_Non_Numeric_Json_Value_And_Returns_An_Instance_Of_NullKObject_If_A_Numeric_Value_Is_Expected(string ksqlType, string json)
        {
            // Arrange
            var jsonElement = Deserialize(json);
            var target = KObjectParser.Create(ksqlType);

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlNull>(actual);
        }

        [Theory]
        [InlineData("INTEGER")]
        [InlineData("BIGINT")]
        [InlineData("DOUBLE")]
        [InlineData("DECIMAL")]
        public void Parses_A_Non_Numeric_Json_Value_And_Returns_An_Instance_Of_NullKObject_If_Unable_To_Parse(string ksqlType)
        {
            // Arrange
            const string value = "not-a-number";
            var jsonElement = Deserialize(JsonSerializer.Serialize(value));
            var target = KObjectParser.Create(ksqlType);

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlNull>(actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Parses_A_Boolean_Json_Value_And_Returns_An_Instance_Of_BooleanKObject(bool value)
        {
            // Arrange
            var jsonElement = Deserialize(JsonSerializer.Serialize(value));
            var target = KObjectParser.Create("BOOLEAN");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<bool>(actual);
        }

        [Fact]
        public void Parses_A_Non_Boolean_Json_Value_And_Returns_An_Instance_Of_NullKObject_Type_If_A_Boolean_Is_Expected()
        {
            // Arrange
            var jsonElement = Deserialize("42");
            var target = KObjectParser.Create("BOOLEAN");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlNull>(actual);
        }

        [Fact]
        public void Parses_A_String_Json_Value_And_Returns_An_Instance_Of_StringKObject()
        {
            // Arrange
            const string value = "abcd";
            var jsonElement = Deserialize(JsonSerializer.Serialize(value));
            var target = KObjectParser.Create("STRING");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<string>(actual);
        }

        [Fact]
        public void Parses_A_Non_String_Json_Value_And_Returns_An_Instance_Of_NullKObject_Type_Of_String_Is_Expected()
        {
            // Arrange
            var jsonElement = Deserialize("42");
            var target = KObjectParser.Create("STRING");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlNull>(actual);
        }

        [Fact]
        public void Parses_A_Json_Array_With_Long_Values_And_Returns_An_Instance_Of_KSqlArray()
        {
            // Arrange
            long[] array = {1, 2, 3, 4};
            var jsonElement = Deserialize(JsonSerializer.Serialize(array));
            var target = KObjectParser.Create("ARRAY<BIGINT>");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlArray>(actual);

            if (actual is not KSqlArray actualArray) return;
            var expected = array.Cast<object>();
            Assert.Equal(expected, actualArray.AsReadOnlyList());
        }

        [Fact]
        public void Parses_A_Json_Array_With_Map_Values_And_Returns_An_Instance_Of_KSqlArray()
        {
            // Arrange
            var firstItem = new Dictionary<string, int> { ["key11"] = 1, ["key12"] = 2 };
            var secondItem = new Dictionary<string, int> { ["key21"] = 3, ["key22"] = 4 };
            var array = new[] {firstItem, secondItem};
            var jsonElement = Deserialize(JsonSerializer.Serialize(array));
            var target = KObjectParser.Create("ARRAY<MAP<STRING, INTEGER>>");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlArray>(actual);
            if (actual is not KSqlArray actualArray) return;
            Assert.All(actualArray.AsReadOnlyList(), item => Assert.IsType<KSqlObject>(item));
            Assert.Collection(actualArray.AsReadOnlyList().OfType<KSqlObject>(),
           item => AssertMap(firstItem, item),
                              item => AssertMap(secondItem, item));
        }

        [Fact]
        public void Parses_A_Json_Map_With_Int_Values_And_Returns_An_Instance_Of_KSqlObject()
        {
            // Arrange
            var map = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };
            var jsonElement = Deserialize(JsonSerializer.Serialize(map));
            var target = KObjectParser.Create("MAP<STRING, INTEGER>");

            // Act
            var actual = target.Parse(jsonElement);

            // Assert
            Assert.IsType<KSqlObject>(actual);
            if (actual is not KSqlObject actualMap) return;
            AssertMap(map, actualMap);
        }

        private static void AssertMap(Dictionary<string, int> expected, KSqlObject actual)
        {
            Assert.Equal(expected.Keys, actual.FieldNames);
            Assert.All(actual.FieldNames, key => Assert.Equal(expected[key], actual.TryGetInteger(key)));
        }

        private static JsonElement Deserialize(string json) => (JsonElement)JsonSerializer.Deserialize<object>(json);
    }
}
