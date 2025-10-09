using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raytracer.Core;

namespace Raytracer.IO.SceneLoaders.Converters;

public class RectConverter : JsonConverter<Rect>
{
    public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        var p = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new Rect
        {
            XMin = float.Parse(p[0]),
            XMax = float.Parse(p[1]),
            YMin = float.Parse(p[2]),
            YMax = float.Parse(p[3])
        };
    }

    public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.XMin} {value.XMax} {value.YMin} {value.YMax}");
    }
}