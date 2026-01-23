using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Rendering.PathTracing.BSDFs;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Scenes.Content;

public struct Materials
{
    [JsonConverter(typeof(SingleOrListConverter<Material>))]
    public List<Material> Material;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Materials:");
        foreach (var material in Material)
            sb.AppendLine(material.ToString());
        return sb.ToString();
    }
    
    public void CreateBRDFs(BrdfLibrary brdfLibrary)
    {
        foreach (var material in Material)
        {
            material.Brdf = brdfLibrary.CreateBRDF(material);
            material.Bsdf = CreateBSDF(material)!;
        }
    }

    private static IBSDF CreateBSDF(Material mat)
    {
        return mat.Type switch
        {
            MaterialType.Mirror => new MirrorBSDF(mat),
            MaterialType.Conductor => new ConductorBSDF(mat),
            MaterialType.Dielectric => new DielectricBSDF(mat),
            _ => new DiffuseBSDF(mat)
        };
    }
}