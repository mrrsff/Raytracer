using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raytracer.IO.SceneLoaders.Converters;

public class HashSetEnumConverter<T> : JsonConverter<HashSet<T>> where T : struct, Enum
{
    public override HashSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Format: "<EnumValue1> <EnumValue2> ..."
        var set = new HashSet<T>();
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumString = reader.GetString();
            if (enumString != null)
            {
                var values = enumString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    var enumValue = ParseString(value);
                    set.Add(enumValue);
                }
            }
        }
        
        return set;
    }

    private T ParseString(string? value)
    {
        if (value != null && Enum.TryParse<T>(value, true, out var result))
        {
            return result;
        }
        throw new JsonException($"Unable to convert \"{value}\" to enum {typeof(T)}.");
    }

    public override void Write(Utf8JsonWriter writer, HashSet<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}