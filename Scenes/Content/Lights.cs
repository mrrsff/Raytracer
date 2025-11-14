using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.SceneLoaders.Converters;

namespace Raytracer.Scenes.Content;

public struct Lights
{
    public Vector3 AmbientLight;

    [JsonConverter(typeof(SingleOrListConverter<PointLight>))]
    public List<PointLight> PointLight;
    
    [JsonConverter(typeof(SingleOrListConverter<AreaLight>))]
    public List<AreaLight> AreaLight;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Ambient Light: {AmbientLight}");
        sb.AppendLine("Point Lights:");
        foreach (var light in PointLight)
            sb.AppendLine(light.ToString());
        sb.AppendLine("Area Lights:");
        foreach (var light in AreaLight)
            sb.AppendLine(light.ToString());
        return sb.ToString();
    }
}