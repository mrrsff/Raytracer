using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public class BVHNode : Geometry
{
    public Geometry left;
    public Geometry right;
    public BoundingBox boundingBox;
    
    public BVHNode(Geometry left, Geometry right, BoundingBox boundingBox)
    {
        this.left = left;
        this.right = right;
        this.boundingBox = boundingBox;
    }

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        if (!boundingBox.Intersects(ray, out float tmin, out float tmax))
            return false;
        
        // visualize the bounding box itself
        if (Debug.ShowBVHBoxes)
        {
            if (TryRenderBoxIntersection(ray, ref info))
                return true; // early return for visible boxes
        }
        
        var leftInfo = IntersectionInfo.NoHit;
        var rightInfo = IntersectionInfo.NoHit;

        bool? hitLeft = left?.Intersect(ray, ref leftInfo);
        bool? hitRight = right?.Intersect(ray, ref rightInfo);

        if (hitLeft == true || hitRight == true)
        {
            info = leftInfo.Distance < rightInfo.Distance ? leftInfo : rightInfo;
            return true;
        }
        return false;
    }
    private bool TryRenderBoxIntersection(in Ray ray, ref IntersectionInfo info)
    {
        if (!boundingBox.IntersectEdge(ray, out float t))
            return false;
        
        if (t < 0) // intersection is behind the ray origin
            return false;

        info.Distance = t;
        info.Point = ray.Origin + ray.Direction * t;
        info.Normal = Vector3.Normalize(info.Point - boundingBox.Center);
        info.Hit = true;
        info.HitRay = ray;
        info.material = Debug.DebugMaterial;
        return true;
    }
}