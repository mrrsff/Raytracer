using System.Text.Json;
using System.Text.Json.Serialization;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.IO.SceneLoaders.Converters;

public class TMOOptionsConverter : JsonConverter<TMOOptions>
{
    public override TMOOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // read string and parse as float array
        var s = reader.GetString();
        var p = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tmoOptions = new TMOOptions
        {
            Params = new float[p.Length]
        };
        for (int i = 0; i < p.Length; i++)
        {
            tmoOptions.Params[i] = float.Parse(p[i]);
        }
        return tmoOptions;
    }

    public override void Write(Utf8JsonWriter writer, TMOOptions value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var param in value.Params)
        {
            writer.WriteNumberValue(param);
        }
        writer.WriteEndArray();
    }
}