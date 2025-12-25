using System.Numerics;
using System.Text;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.CameraData;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Utility;

namespace Raytracer.Scenes.Content;

public class SceneContent
{
    public Vector3 BackgroundColor = ColorUtility.Black;

    public Cameras Cameras;
    public float IntersectionTestEpsilon = 1e-6f;
    public Lights Lights;
    public Materials Materials;
    public int MaxRecursionDepth = 1;
    public Objects Objects;
    public float ShadowRayEpsilon = 1e-3f;
    public Transformations Transformations;
    public VertexData VertexData;
    public Textures Textures;
    public TexCoordData TexCoordData;
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Background Color: {BackgroundColor}");
        sb.AppendLine($"Shadow Ray Epsilon: {ShadowRayEpsilon}");
        sb.AppendLine($"Intersection Test Epsilon: {IntersectionTestEpsilon}");
        sb.AppendLine($"Max Recursion Depth: {MaxRecursionDepth}");
        sb.AppendLine(Cameras.ToString());
        sb.AppendLine(Lights.ToString());
        sb.AppendLine(Materials.ToString());
        sb.AppendLine(Transformations.ToString());
        sb.AppendLine(VertexData.ToString());
        sb.AppendLine(Objects.ToString());
        sb.AppendLine(Textures.ToString());
        sb.AppendLine(TexCoordData.ToString());
        return sb.ToString();
    }
}