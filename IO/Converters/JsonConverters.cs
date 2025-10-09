using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raytracer.Scenes.Datas;

namespace Raytracer.IO.Converters;

    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) return Vector3.Zero;
            var p = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        }
        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
            => writer.WriteStringValue($"{value.X} {value.Y} {value.Z}");
    }

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
            => writer.WriteStringValue($"{value.XMin} {value.XMax} {value.YMin} {value.YMax}");
    }

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
            => writer.WriteStringValue($"{value.Width} {value.Height}");
    }

    public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
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
            => JsonSerializer.Serialize(writer, value, options);
    }