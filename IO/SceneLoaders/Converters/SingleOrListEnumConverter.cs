using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raytracer.IO.SceneLoaders.Converters;

public class SingleOrListEnumConverter<T> : JsonConverter<List<T>> where T : struct, Enum
{
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<T>();

        if (reader.TokenType == JsonTokenType.String)
        {
            // Logic from StringToEnumConverter: Handle single string
            list.Add(ParseString(reader.GetString()));
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Logic from SingleOrListConverter: Handle array
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    list.Add(ParseString(reader.GetString()));
                }
            }
        }
        else
        {
            throw new JsonException($"Unexpected token {reader.TokenType} for Enum conversion.");
        }

        return list;
    }

    private T ParseString(string? value)
    {
        if (value != null && Enum.TryParse<T>(value, true, out var result))
        {
            return result;
        }
        throw new JsonException($"Unable to convert \"{value}\" to enum {typeof(T)}.");
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}