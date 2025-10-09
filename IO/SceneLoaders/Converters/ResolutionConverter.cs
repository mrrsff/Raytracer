using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raytracer.Core;

namespace Raytracer.IO.SceneLoaders.Converters;

public class ResolutionConverter : JsonConverter<Resolution>
{
    public override Resolution Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        var p = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new Resolution
        {
            Width = int.Parse(p[0]),
            Height = int.Parse(p[1])
        };
    }

    public override void Write(Utf8JsonWriter writer, Resolution value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.Width} {value.Height}");
    }
}