using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public class BoundingVolumeHierarchy(MeshDefinition meshDefinition) : Geometry
{
    private readonly BVHNode[] nodes = BVHBuilder.Build(meshDefinition);
    private readonly Triangle[] triangles = meshDefinition.Triangles;

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        // int TriangleIntersectionTests = 0;
        // int BoundingBoxTests = 0;
        
        bool hit = false;
        float closest = float.MaxValue;
        IntersectionInfo temp = IntersectionInfo.NoHit;

        Stack<int> stack = new Stack<int>();
        stack.Push(0);

        while (stack.Count > 0)
        {
            int i = stack.Pop();
            ref var node = ref nodes[i];

            if (!node.IsLeaf)
            {
                bool hitL = nodes[node.LeftChild].Bounds.Intersects(ray, out var tMinL, out _);
                bool hitR = nodes[node.RightChild].Bounds.Intersects(ray, out var tMinR, out _);
                // BoundingBoxTests += 2;
                if (hitL && hitR)
                {
                    if (tMinL < tMinR)
                    {
                        stack.Push(node.RightChild);
                        stack.Push(node.LeftChild);
                    }
                    else
                    {
                        stack.Push(node.LeftChild);
                        stack.Push(node.RightChild);
                    }
                }
                else if (hitL) stack.Push(node.LeftChild);
                else if (hitR) stack.Push(node.RightChild);
            }
            else
            {
                for (int t = node.Start; t < node.End; t++)
                {
                    temp.Reset();
                    if (triangles[t].Intersect(ray, ref temp) && temp.Distance < closest)
                    {
                        closest = temp.Distance;
                        info = temp;
                        hit = true;
                    }
                }
                // TriangleIntersectionTests += (node.End - node.Start);
            }
        }

        // if (hit && TriangleIntersectionTests > 1000)
        // {
        //     // Debug stats
        //     Console.WriteLine($"BVH Intersection: Triangle Tests = {TriangleIntersectionTests}, Bounding Box Tests = {BoundingBoxTests}" );
        // }
        return hit;
    }
}