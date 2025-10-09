using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Scenes;

namespace Raytracer.IO.SceneLoaders;

public static class SceneLoader
{
    public static Scene Load(string path)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new Vector3Converter(),
                new RectConverter(),
                new ResolutionConverter(),
                new FloatConverter(),
                new IntConverter(),
                new IntArrayConverter(),
                new FloatArrayConverter()
            }
        };

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Scene>(json, options) ?? throw new Exception("Failed to deserialize scene");
    }
}