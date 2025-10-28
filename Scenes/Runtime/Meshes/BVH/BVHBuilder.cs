using System.Diagnostics;
using System.Numerics;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public static class BVHBuilder
{
    private static int minTrianglesPerNode = 2;
    private static int maxRecursionDepth = 20;
    public static BVHNode Build(MeshDefinition meshDefinition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var triangles = meshDefinition.Triangles;
        var root = BuildRecursive(ref triangles, 0, triangles.Length);
        stopwatch.Stop();
        Console.WriteLine($"BVH built in {stopwatch.ElapsedMilliseconds} ms");
        return root;
    }
    private static BVHNode BuildRecursive(ref Triangle[] triangles, int start, int end, int depth = 0)
    {
        int triangleCount = end - start;
        if (triangleCount <= minTrianglesPerNode || depth >= maxRecursionDepth)
        {
            var leafBox = ComputeBoundingBox(triangles, start, end);
            var leafNode = new BVHNode(new TriangleGeometry(triangles, start, end), null, leafBox);
            return leafNode;
        }

        var boundingBox = ComputeBoundingBox(triangles, start, end);
        int axis = boundingBox.LargestAxis();
        float splitPosition = boundingBox.Center[axis];

        int mid = PartitionTriangles(ref triangles, start, end, axis, splitPosition);

        if (mid == start || mid == end)
        {
            mid = start + (end - start) / 2;
        }

        var leftNode = BuildRecursive(ref triangles, start, mid, depth + 1);
        var rightNode = BuildRecursive(ref triangles, mid, end, depth + 1);

        var nodeBox = ComputeBoundingBox(triangles, start, end);
        return new BVHNode(leftNode, rightNode, nodeBox);
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
            while (i <= j && triangles[i].Centroid[axis] < splitPosition)
                i++;
            while (i <= j && triangles[j].Centroid[axis] >= splitPosition)
                j--;

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
        if (size.X > size.Y && size.X > size.Z)
            return 0;
        if (size.Y > size.Z)
            return 1;
        return 2;
    }
}