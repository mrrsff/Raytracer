using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raytracer.IO.SceneLoaders.Converters;

public class SingleOrListConverter<T> : JsonConverter<List<T>>
{
    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<T>();
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                list.Add(JsonSerializer.Deserialize<T>(ref reader, options));
                break;
            case JsonTokenType.StartArray:
                list.AddRange(JsonSerializer.Deserialize<List<T>>(ref reader, options));
                break;
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}