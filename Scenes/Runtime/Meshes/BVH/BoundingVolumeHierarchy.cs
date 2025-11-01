using Raytracer.Core;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public class BoundingVolumeHierarchy(MeshDefinition meshDefinition) : Geometry
{
    private readonly BVHNodeFlat[] nodes = BVHBuilder.Build(meshDefinition);
    private readonly Triangle[] triangles = meshDefinition.Triangles;

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        bool hit = false;
        float closest = float.MaxValue;
        IntersectionInfo temp = IntersectionInfo.NoHit;

        Stack<int> stack = new Stack<int>();
        stack.Push(0);

        while (stack.Count > 0)
        {
            int i = stack.Pop();
            var node = nodes[i];

            if (!node.Bounds.Intersects(ray, out float tmin, out float tmax))
                continue;
            if (tmax < 0) continue;

            if (node.IsLeaf)
            {
                for (int t = node.Start; t < node.End; t++)
                {
                    temp = IntersectionInfo.NoHit;
                    if (triangles[t].Intersect(ray, ref temp) && temp.Distance < closest)
                    {
                        closest = temp.Distance;
                        info = temp;
                        hit = true;
                    }
                }
                continue;
            }

            // Push children
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
        }
        return hit;
    }
}