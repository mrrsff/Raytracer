using System.Text;
using System.Text.Json.Serialization;
using Raytracer.IO.SceneLoaders.Converters;

namespace Raytracer.Scenes.Content.Datas.CameraData;

public struct Cameras
{
    [JsonConverter(typeof(SingleOrListConverter<Camera>))]
    public List<Camera> Camera;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Cameras:");
        foreach (var camera in Camera)
            sb.AppendLine(camera.ToString());
        return sb.ToString();
    }
}