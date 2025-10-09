using System.Text.Json.Serialization;
using Raytracer.Scenes.Content;

namespace Raytracer.Scenes;

public class Scene
{
    [JsonPropertyName("Scene")] public SceneContent Content;

    public Scene()
    {
    }

    public Scene(SceneContent content)
    {
        Content = content;
    }
}