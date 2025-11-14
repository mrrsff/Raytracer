using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Utility;

namespace Raytracer.Core;

public class AreaLight
{
    [JsonPropertyName("_id")] public int Id;
    public string Transformations;
    public Vector3 Position;
    public Vector3 Normal;
    public float Extent;
    public Vector3 Radiance;
    
    public Transform Transform;
    
    public float Area => Extent * Extent;

    public Vector3 U = Vector3.Zero;
    public Vector3 V = Vector3.Zero;
    
    public override string ToString()
    {
        return new StringBuilder().Append("AreaLight(Id: ")
            .Append(Id)
            .Append(", Position: ")
            .Append(Position)
            .Append(", Normal: ")
            .Append(Normal)
            .Append(", Radiance: ")
            .Append(Radiance)
            .Append(", Extent: ")
            .Append(Extent)
            .Append(')').ToString();
    }
    public void CalculateValues()
    {
        // Generate orthonormal basis (U, V) for the area light's plane
        MathUtility.BuildONB(Normal, out U, out V);
    }
}