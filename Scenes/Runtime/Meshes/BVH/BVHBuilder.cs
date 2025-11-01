using System.Diagnostics;
using System.Numerics;
using Debug = Raytracer.Core.Debug;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public static class BVHBuilder
{
    public static BVHNodeFlat[] Build(MeshDefinition meshDefinition)
    {
        var triangles = meshDefinition.Triangles;
        int triangleCount = triangles.Length;

        // adaptive params
        int minTris = triangleCount < 10_000 ? 32 :
            triangleCount < 100_000 ? 16 : 
            triangleCount < 1000_000 ? 8 : 4;
        int maxDepth = triangleCount < 10_000 ? 12 :
            triangleCount < 100_000 ? 16 :
            triangleCount < 1000_000 ? 20 : 24;

        var nodes = new List<BVHNodeFlat>(triangleCount * 2);
        Stopwatch sw = Stopwatch.StartNew();
        BuildRecursive(ref triangles, 0, triangleCount, 0, minTris, maxDepth, nodes);
        sw.Stop();
        if (Debug.DebugBVHBuildTime) Console.WriteLine($"BVH built in {sw.ElapsedMilliseconds} ms with {nodes.Count} nodes for {triangleCount} triangles.");
        return nodes.ToArray();
    }

    private static int BuildRecursive(ref Triangle[] tris, int start, int end, int depth,
        int minTris, int maxDepth, List<BVHNodeFlat> nodes)
    {
        int nodeIndex = nodes.Count;

        var bbox = ComputeBoundingBox(tris, start, end);
        BVHNodeFlat node = new BVHNodeFlat
        {
            Bounds = bbox,
            Start = start,
            End = end,
            LeftChild = -1,
            RightChild = -1
        };
        nodes.Add(node);

        int triCount = end - start;
        if (triCount <= minTris || depth >= maxDepth)
            return nodeIndex;

        int axis = bbox.LargestAxis();
        float split = bbox.Center[axis];
        int mid = PartitionTriangles(ref tris, start, end, axis, split);
        if (mid == start || mid == end)
            mid = start + (end - start) / 2;

        int left = BuildRecursive(ref tris, start, mid, depth + 1, minTris, maxDepth, nodes);
        int right = BuildRecursive(ref tris, mid, end, depth + 1, minTris, maxDepth, nodes);

        BVHNodeFlat updated = nodes[nodeIndex];
        updated.LeftChild = left;
        updated.RightChild = right;
        nodes[nodeIndex] = updated;

        return nodeIndex;
    }
    private static BoundingBox ComputeBoundingBox(Triangle[] triangles, int start, int end)
    {
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);

        for (int i = start; i < end; i++)
        {
            var tri = triangles[i];
            min = Vector3.Min(min, Vector3.Min(tri.V0, Vector3.Min(tri.V1, tri.V2)));
            max = Vector3.Max(max, Vector3.Max(tri.V0, Vector3.Max(tri.V1, tri.V2)));
        }

        return new BoundingBox(min, max);
    }
    private static int PartitionTriangles(ref Triangle[] triangles, int start, int end, int axis, float splitPosition)
    {
        int i = start;
        int j = end - 1;

        while (i <= j)
        {
            while (i <= j && triangles[i].Centroid[axis] < splitPosition) i++;
            while (i <= j && triangles[j].Centroid[axis] >= splitPosition) j--;

            if (i >= j) continue;
            (triangles[i], triangles[j]) = (triangles[j], triangles[i]);
            i++;
            j--;
        }

        return i;
    }

    private static int LargestAxis(this BoundingBox box)
    {
        Vector3 size = box.Max - box.Min;
        if (size.X > size.Y && size.X > size.Z) return 0;
        if (size.Y > size.Z) return 1;
        return 2;
    }
}
