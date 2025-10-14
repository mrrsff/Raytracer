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
            Left = float.Parse(p[0]),
            Right = float.Parse(p[1]),
            Bottom = float.Parse(p[2]),
            Top = float.Parse(p[3])
        };
    }

    public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.Left} {value.Right} {value.Bottom} {value.Top}");
    }
}