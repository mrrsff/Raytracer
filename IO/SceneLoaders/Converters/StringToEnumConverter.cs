using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raytracer.IO.SceneLoaders.Converters;

public class StringToEnumConverter<T> : JsonConverter<T> where T : Enum
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? enumString = reader.GetString();
            if (enumString != null && Enum.TryParse(typeof(T), enumString, true, out var enumValue))
            {
                return (T)enumValue;
            }
        }

        throw new JsonException($"Unable to convert to enum of type {typeof(T)}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}