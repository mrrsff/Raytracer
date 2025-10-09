using System.Numerics;
using System.Text;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Scenes.Content;

public class SceneContent
{
    public Vector3 BackgroundColor;

    public Cameras Cameras;
    public float IntersectionTestEpsilon;
    public Lights Lights;
    public Materials Materials;
    public int MaxRecursionDepth;
    public Objects Objects;
    public float ShadowRayEpsilon;
    public VertexData VertexData;

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
        sb.AppendLine(VertexData.ToString());
        sb.AppendLine(Objects.ToString());
        return sb.ToString();
    }
}