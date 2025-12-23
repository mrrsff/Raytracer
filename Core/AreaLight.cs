using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Rendering.CPU.RenderingDebug;
using Raytracer.Utility;

namespace Raytracer.Core;

public class AreaLight
{
    [JsonPropertyName("_id")] public int Id;
    public string Transformations;
    public Vector3 Position;
    public Vector3 Normal;
    public float Size;
    public Vector3 Radiance;
    
    public Transform Transform;
    
    public float Area => Size * Size;

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
            .Append(", Size: ")
            .Append(Size)
            .Append(')').ToString();
    }
    public void CalculateValues()
    {
        // Generate orthonormal basis (U, V) for the area light's plane
        MathUtility.BuildONB(Normal, out U, out V);
        
        var positions = new Vector3[]
        {
            Position + ( U + V) * ( Size * 0.5f),
            Position + (-U + V) * ( Size * 0.5f),
            Position + (-U - V) * ( Size * 0.5f),
            Position + ( U - V) * ( Size * 0.5f),
        };
        
        // draw all lines
        for (int i = 0; i < 4; i++)
        {
            DebugRenderer.Add(new DrawLine(positions[i], positions[(i + 1) % 4], ColorUtility.Yellow));
        }
        
        // draw normal as arrow
        DebugRenderer.Add(new DrawArrow(Position, Position + Normal * 2, ColorUtility.Cyan));
    }
}