using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Triangle : Geometry
{
    public int PrimitiveIndex;
    public int I0, I1, I2;
    public Vector3 V0, V1, V2;
    public Vector3 Centroid { get; private set; }
    
    public Vector3 E1; // V1 - V0
    public Vector3 E2; // V2 - V0

    public Vector3 Normal;
    
    public Triangle(int primitiveIndex, int i0, int i1, int i2, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        PrimitiveIndex = primitiveIndex;
        I0 = i0;
        I1 = i1;
        I2 = i2;
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        E1 = v1 - v0;
        E2 = v2 - v0;
    }
    
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var hit = TriangleIntersection.Intersect(ray, V0, V1, V2, Normal, ref info);
        info.PrimitiveIndex = PrimitiveIndex;
        return hit;
    }
    
    public void CalculateBarycentricCoordinates(in Vector3 point, out float alpha, out float beta, out float gamma)
    {
        Vector3 v0 = E1;               // edge1 = V1 - V0
        Vector3 v1 = E2;               // edge2 = V2 - V0
        Vector3 v2 = point - V0;       // vector from V0 to point

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;

        beta  = (d11 * d20 - d01 * d21) / denom;
        gamma = (d00 * d21 - d01 * d20) / denom;
        alpha = 1.0f - beta - gamma;
    }

}