using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Runtime.Meshes;

namespace Raytracer.Scenes.Runtime;

public class Triangle : Geometry
{
    public int PrimitiveIndex;
    public readonly int I0, I1, I2;
    public Vector3 V0 => MeshDefinition.Vertices[I0];
    public Vector3 V1 => MeshDefinition.Vertices[I1];
    public Vector3 V2 => MeshDefinition.Vertices[I2];
    public Vector3 Centroid;

    private Vector3 E1; // V1 - V0
    private Vector3 E2; // V2 - V0
    private MeshDefinition MeshDefinition;

    public Vector3 Normal;

    public Triangle(int primitiveIndex, int i0, int i1, int i2, MeshDefinition meshDefinition)
    {
        MeshDefinition = meshDefinition;
        PrimitiveIndex = primitiveIndex;
        I0 = i0;
        I1 = i1;
        I2 = i2;
        Normal = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
        E1 = V1 - V0;
        Centroid = (V0 + V1 + V2) / 3.0f;
        Bounds = new BoundingBox(
            Vector3.Min(Vector3.Min(V0, V1), V2),
            Vector3.Max(Vector3.Max(V0, V1), V2)
        );
    }

    public Triangle(int primitiveIndex, int i0, int i1, int i2)
    {
        PrimitiveIndex = primitiveIndex;
        I0 = i0;
        I1 = i1;
        I2 = i2;
    }

    public void SetMeshDefinition(MeshDefinition meshDefinition)
    {
        MeshDefinition = meshDefinition;
        Normal = Vector3.Normalize(Vector3.Cross(V1 - V0, V2 - V0));
        E1 = V1 - V0;
        E2 = V2 - V0;
        Centroid = (V0 + V1 + V2) / 3.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var hit = TriangleIntersection.Intersect(ray, V0, V1, V2, Normal, ref info);
        info.HitGeometry = this;
        info.PrimitiveIndex = PrimitiveIndex;
        return hit;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CalculateBarycentricCoordinates(in Vector3 point, out float alpha, out float beta, out float gamma)
    {
        Vector3 v0 = E1; // edge1 = V1 - V0
        Vector3 v1 = E2; // edge2 = V2 - V0
        Vector3 v2 = point - V0; // vector from V0 to point

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;

        beta = (d11 * d20 - d01 * d21) / denom;
        gamma = (d00 * d21 - d01 * d20) / denom;
        alpha = 1.0f - beta - gamma;
    }
}