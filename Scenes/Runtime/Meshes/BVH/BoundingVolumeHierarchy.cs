using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public class BoundingVolumeHierarchy : Geometry
{
    private readonly BVHNode[] nodes;
    private readonly Geometry[] geometries;
    public ref BVHNode GetNode(int i) => ref nodes[i];
    public int NodeCount => nodes.Length;

    public BoundingVolumeHierarchy(MeshDefinition meshDefinition)
    {
        nodes = BVHBuilder.Build(meshDefinition);
        var tris = meshDefinition.Triangles;
        geometries = new Geometry[tris.Length];
        for (int i = 0; i < tris.Length; i++) geometries[i] = tris[i];
        
        if (Debug.ShowBVHBoxes) DrawDebugBVH();
    }

    public BoundingVolumeHierarchy(Scene scene)
    {
        nodes = BVHBuilder.Build(scene, out geometries);
        
        if (Debug.ShowTLASBoxes) DrawDebugBVH();
    }

    private void DrawDebugBVH()
    {
        DebugRenderer.CollectBVH(this, Debug.ShowBVHBoxesLeafNodesOnly);
    }
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
            ref var node = ref nodes[i];

            if (!node.IsLeaf)
            {
                bool hitL = nodes[node.LeftChild].Bounds.Intersects(ray, out var tMinL, out _);
                bool hitR = nodes[node.RightChild].Bounds.Intersects(ray, out var tMinR, out _);
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
                    if (geometries[t].Intersect(ray, ref temp) && temp.Distance < closest)
                    {
                        closest = temp.Distance;
                        info = temp;
                        info.HitGeometry = geometries[t];
                        hit = true;
                    }
                }
            }
        }
        return hit;
    }
}