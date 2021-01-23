using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using KsqlDb.Api.Client.Abstractions.Objects;
using Serializer = System.Text.Json.JsonSerializer;

namespace KsqlDb.Api.Client.Serdes
{
    internal class KSqlArrayConverter : JsonConverter<KSqlArray>
    {
        public override KSqlArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, KSqlArray value, JsonSerializerOptions options) => Serializer.Serialize(writer, value.AsReadOnlyList(), options);
    }
}
