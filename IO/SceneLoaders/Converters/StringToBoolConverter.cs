using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raytracer.IO.SceneLoaders.Converters;

public class StringToBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? str = reader.GetString();
            if (str != null)
            {
                return str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                       str.Equals("1");
            }
        }
        else if (reader.TokenType == JsonTokenType.True)
        {
            return true;
        }
        else if (reader.TokenType == JsonTokenType.False)
        {
            return false;
        }

        throw new JsonException("Invalid value for boolean conversion.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value ? "true" : "false");
    }
}