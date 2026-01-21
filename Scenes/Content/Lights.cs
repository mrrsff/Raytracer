using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core.Lights;
using Raytracer.IO.SceneLoaders.Converters;

namespace Raytracer.Scenes.Content;

public struct Lights
{
    public Vector3 AmbientLight;

    [JsonConverter(typeof(SingleOrListConverter<PointLight>))]
    public List<PointLight> PointLight;
    
    [JsonConverter(typeof(SingleOrListConverter<AreaLight>))]
    public List<AreaLight> AreaLight;
    
    [JsonConverter(typeof(SingleOrListConverter<SpotLight>))]
    public List<SpotLight> SpotLight;
    
    [JsonConverter(typeof(SingleOrListConverter<DirectionalLight>))]
    public List<DirectionalLight> DirectionalLight;
    
    [JsonConverter(typeof(SingleOrListConverter<SphericalDirectionalLight>))]
    public List<SphericalDirectionalLight> SphericalDirectionalLight;

    public List<ILight> AllLights;
    
    public void Initialize()
    {
        AllLights = [];
        if (PointLight != null)
            AllLights.AddRange(PointLight);
        if (AreaLight != null)
            AllLights.AddRange(AreaLight);
        if (SpotLight != null)
            AllLights.AddRange(SpotLight);
        if (DirectionalLight != null)
            AllLights.AddRange(DirectionalLight);
        if (SphericalDirectionalLight != null)
            AllLights.AddRange(SphericalDirectionalLight);
    }
    public List<PointLight> GetPointLights() => PointLight ?? [];
    public List<AreaLight> GetAreaLights() => AreaLight ?? [];
    public List<SpotLight> GetSpotLights() => SpotLight ?? [];
    public List<DirectionalLight> GetDirectionalLights() => DirectionalLight ?? [];
    public List<SphericalDirectionalLight> GetSphericalDirectionalLights() => SphericalDirectionalLight ?? [];
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Ambient Light: {AmbientLight}");
        if (PointLight != null)
        {
            sb.AppendLine("Point Lights:");
            foreach (var light in PointLight)
                sb.AppendLine(light.ToString());
        }
        if (AreaLight != null)
        {
            sb.AppendLine("Area Lights:");
            foreach (var light in AreaLight)
                sb.AppendLine(light.ToString());   
        }
        if (SpotLight != null)
        {
            sb.AppendLine("Spot Lights:");
            foreach (var light in SpotLight)
                sb.AppendLine(light.ToString());   
        }
        if (DirectionalLight != null)
        {
            sb.AppendLine("Directional Lights:");
            foreach (var light in DirectionalLight)
                sb.AppendLine(light.ToString());   
        }
        if (SphericalDirectionalLight != null)
        {
            sb.AppendLine("Spherical Directional Lights:");
            foreach (var light in SphericalDirectionalLight)
                sb.AppendLine(light.ToString());   
        }
        return sb.ToString();
    }
}