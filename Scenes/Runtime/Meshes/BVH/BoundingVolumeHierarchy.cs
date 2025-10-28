using Raytracer.Core;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public class BoundingVolumeHierarchy(MeshDefinition meshDefinition) : Geometry
{
    private readonly BVHNode Root = BVHBuilder.Build(meshDefinition);

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        return Root.Intersect(in ray, ref info);
    }
}