using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using KsqlDb.Api.Client.Abstractions.Objects;

namespace KsqlDb.Api.Client.Serdes
{
    internal class KSqlNullConverter : JsonConverter<KSqlNull>
    {
        public override KSqlNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, KSqlNull value, JsonSerializerOptions options) => writer.WriteNullValue();
    }
}
