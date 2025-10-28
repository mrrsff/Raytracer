using Raytracer.Core;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime;

public class TriangleGeometry : Geometry
{
    private readonly Memory<Triangle> triangles;
    private int start;
    private int end;

    public TriangleGeometry(Triangle[] source, int start, int end)
        => triangles = source.AsMemory(start, end - start);

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        IntersectionInfo closestInfo = IntersectionInfo.NoHit;
        foreach (var tri in triangles.Span) 
        {
            if (tri.Intersect(in ray, ref closestInfo))
            {
                if (closestInfo.Distance < info.Distance)
                {
                    info = closestInfo;
                }
            }
        }
        return info.Hit;
    }

    public override string ToString()
    {
        return $"TriangleGeometry(Triangles: {end - start})";
    }
}