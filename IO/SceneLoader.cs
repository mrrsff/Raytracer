using System.IO;
using System.Text.Json;
using Raytracer.IO.Converters;
using Raytracer.Scenes;

namespace Raytracer.IO;

public class SceneLoader
{
    public static Scene Load(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new Vector3Converter(),
                new RectConverter(),
                new ResolutionConverter()
            }
        };

        string json = File.ReadAllText(path);
        var content = JsonSerializer.Deserialize<SceneContent>(json, options)!;
        return new Scene(content);
    }
}