using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Debug = Raytracer.Core.Debug;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

public static class BVHBuilder
{
    private const float C_trav = 0.125f;
    private const float C_isect = 1f;

    #region Mesh BVH Build
    public static BVHNode[] Build(MeshDefinition meshDefinition)
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

        var nodes = new List<BVHNode>(triangleCount * 2);
        Stopwatch sw = Stopwatch.StartNew();
        if (Debug.UseParallelBVHBuild)
            BuildParallel(triangles, 0, triangleCount, minTris, maxDepth, nodes);
        else
            BuildIterative(ref triangles, 0, triangleCount, minTris, maxDepth, nodes);
        
        sw.Stop();
        if (Debug.DebugBVHBuildTime)
        {
            string mode = Debug.UseParallelBVHBuild ? "Parallel" : "Iterative";
            Console.WriteLine(
                $"{mode} BVH built in {sw.ElapsedMilliseconds} ms with {nodes.Count} nodes for {triangleCount} triangles.");
        }
        return nodes.ToArray();
    }
    
    #region Iterative BVH Build
    struct BuildTask
    {
        public int Start, End, Depth, ParentIndex;
        public bool IsLeftChild;
    }
    private static int BuildIterative(ref Triangle[] tris, int start, int end, int minTris, int maxDepth, List<BVHNode> nodes)
    {
        Stack<BuildTask> stack = new();
        stack.Push(new BuildTask { Start = start, End = end, Depth = 0, ParentIndex = -1, IsLeftChild = false });
    
        int rootIndex = -1;
    
        while (stack.Count > 0)
        {
            var task = stack.Pop();
    
            // Build node
            int nodeIndex = nodes.Count;
            var bbox = ComputeBoundingBox(tris, task.Start, task.End);
    
            BVHNode node = new BVHNode
            {
                Bounds = bbox,
                Start = task.Start,
                End = task.End,
                LeftChild = -1,
                RightChild = -1
            };
            nodes.Add(node);
    
            // Link to parent
            if (task.ParentIndex != -1)
            {
                BVHNode parent = nodes[task.ParentIndex];
                if (task.IsLeftChild) parent.LeftChild = nodeIndex;
                else parent.RightChild = nodeIndex;
                nodes[task.ParentIndex] = parent;
            }
            else
            {
                rootIndex = nodeIndex; // first node = root
            }
    
            int triCount = task.End - task.Start;
            if (triCount <= minTris || task.Depth >= maxDepth)
                continue;
    
            float parentCost = triCount * C_isect;
            var (axis, split, splitCost) = ChooseSplitAxis(ref tris, task.Start, task.End);
    
            if (splitCost >= parentCost && triCount <= minTris)
                continue;
    
            int mid = PartitionTriangles(tris, task.Start, task.End, axis, split);
            if (mid == task.Start || mid == task.End)
            {
                Console.WriteLine($"Warning: SAH split failed at depth {task.Depth}, falling back to median split.");
                int axisFB = bbox.LargestAxis();
                float posFB = 0.5f * (bbox.Min[axisFB] + bbox.Max[axisFB]);
                mid = PartitionTriangles(tris, task.Start, task.End, axisFB, posFB);
                if (mid == task.Start || mid == task.End)
                    continue;
            }
    
            // Push children — push right first so left is processed first (depth-first)
            stack.Push(new BuildTask { Start = mid, End = task.End, Depth = task.Depth + 1, ParentIndex = nodeIndex, IsLeftChild = false });
            stack.Push(new BuildTask { Start = task.Start, End = mid, Depth = task.Depth + 1, ParentIndex = nodeIndex, IsLeftChild = true });
        }
    
        return rootIndex;
    }
    #endregion

    #region Parallel BVH Build
    private static void BuildParallel(Triangle[] tris, int start, int end, int minTris, int maxDepth, List<BVHNode> nodes)
    {
        object nodeLock = new();

        int rootIndex = -1;

        BuildNode(start, end, 0, -1, false);
        return;

        void BuildNode(int start_, int end_, int depth, int parentIdx, bool isLeftChild)
        {
            var bbox = ComputeBoundingBox(tris, start_, end_);
            int nodeIndex;

            lock (nodeLock)
            {
                nodeIndex = nodes.Count;
                nodes.Add(new BVHNode
                {
                    Bounds = bbox,
                    Start = start_,
                    End = end_,
                    LeftChild = -1,
                    RightChild = -1
                });

                if (parentIdx != -1)
                {
                    var p = nodes[parentIdx];
                    if (isLeftChild) p.LeftChild = nodeIndex;
                    else p.RightChild = nodeIndex;
                    nodes[parentIdx] = p;
                }
                else rootIndex = nodeIndex;
            }

            int triCount = end_ - start_;
            if (triCount <= minTris || depth >= maxDepth)
                return;

            float parentCost = triCount * C_isect;
            var (axis, split, splitCost) = ChooseSplitAxis(ref tris, start_, end_);

            if (splitCost >= parentCost)
                return;

            // int axisFB = bbox.LargestAxis();
            // float posFB = 0.5f * (bbox.Min[axisFB] + bbox.Max[axisFB]);
            // int mid = PartitionTriangles(tris, start_, end_, axisFB, posFB);
            
            int mid = PartitionTriangles(tris, start_, end_, axis, split);
            if (mid == start_ || mid == end_)
            {
                Console.WriteLine($"SAH split failed at depth {depth}, falling back to median split.");
                int axisFB = bbox.LargestAxis();
                float posFB = 0.5f * (bbox.Min[axisFB] + bbox.Max[axisFB]);
                mid = PartitionTriangles(tris, start_, end_, axisFB, posFB);
                if (mid == start_ || mid == end_)
                    return;
            }

            bool spawnParallel = triCount > 20000 && depth < maxDepth - 2;

            if (spawnParallel)
            {
                var leftTask = Task.Run(() => BuildNode(start_, mid, depth + 1, nodeIndex, true));
                BuildNode(mid, end_, depth + 1, nodeIndex, false);
                leftTask.Wait();
            }
            else
            {
                BuildNode(start_, mid, depth + 1, nodeIndex, true);
                BuildNode(mid, end_, depth + 1, nodeIndex, false);
            }
        }
    }
    #endregion
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SurfaceArea(in BoundingBox b)
    {
        Vector3 d = b.Max - b.Min;
        if (d.X <= 0 || d.Y <= 0 || d.Z <= 0) return 0f;
        return (d.X * d.Y + d.X * d.Z + d.Y * d.Z);
    }
    private static (int, float, float) ChooseSplitAxis(ref Triangle[] triangles, int start, int end)
    {
        const int numTestsPerAxis = 2;
        float bestCost = float.MaxValue;
        float bestPosition = 0f;
        int bestAxis = 0;
        
        BoundingBox nodeBox = ComputeBoundingBox(triangles, start, end);
        for (int axis = 0; axis < 3; axis++)
        {
            float boundsStart = nodeBox.Min[axis];
            float boundsEnd = nodeBox.Max[axis];
            for (int i = 1; i <= numTestsPerAxis; i++)
            {   
                float splitT = i / (numTestsPerAxis + 1f);
                float pos = boundsStart + (boundsEnd - boundsStart) * splitT;
                float cost = EvaluateSplitCost(triangles, start, end, axis, pos, nodeBox);
                if (cost >= bestCost) continue;
                bestCost = cost;
                bestPosition = pos;
                bestAxis = axis;
            }
        }
        
        return (bestAxis, bestPosition, bestCost);
    }
    private static float EvaluateSplitCost(
        Triangle[] triangles, int start, int end,
        int axis, float splitPosition, BoundingBox parentBox)
    {
        BoundingBox leftBox = BoundingBox.Invalid;
        BoundingBox rightBox = BoundingBox.Invalid;
        int leftCount = 0, rightCount = 0;

        for (int i = start; i < end; i++)
        {
            ref var tri = ref triangles[i];
            if (tri.Centroid[axis] < splitPosition)
            {
                leftBox.Encapsulate(tri);
                leftCount++;
            }
            else
            {
                rightBox.Encapsulate(tri);
                rightCount++;
            }
        }

        if (leftCount == 0 || rightCount == 0) return float.MaxValue;

        float SA_P = SurfaceArea(parentBox);
        float SA_L = SurfaceArea(leftBox);
        float SA_R = SurfaceArea(rightBox);
        if (SA_P == 0f) return float.MaxValue; // degenerate parent
        
        float cost = C_trav
                     + (SA_L / SA_P) * leftCount * C_isect
                     + (SA_R / SA_P) * rightCount * C_isect;
        return cost;
    }
    private static int PartitionTriangles(Triangle[] triangles, int start, int end, int axis, float split)
    {
        int i = start;
        int j = end - 1;

        while (i < j)
        {
            while (i < j && triangles[i].Centroid[axis] < split) i++;
            while (i < j && triangles[j].Centroid[axis] >= split) j--;
            if (i >= j) continue;
            (triangles[i], triangles[j]) = (triangles[j], triangles[i]);
            triangles[i].PrimitiveIndex = i;
            triangles[j].PrimitiveIndex = j;
            i++; j--;
        }
        return i;
    }
    #endregion

    #region Scene BVH Build
    public static BVHNode[] Build(Scene scene, out Geometry[] geometries)
    {
        geometries = scene.Geometries.ToArray();
        int geometryCount = geometries.Length;

        const int minGeos = 2;
        const int maxDepth = 20;

        var nodes = new List<BVHNode>(geometryCount * 2);
        Stopwatch sw = Stopwatch.StartNew();
        
        BuildScene(geometries, 0, geometryCount, minGeos, maxDepth, nodes);

        sw.Stop();
        if (Debug.DebugBVHBuildTime)
        {
            string mode = Debug.UseParallelBVHBuild ? "Parallel" : "Iterative";
            Console.WriteLine(
                $"{mode} TLAS built in {sw.ElapsedMilliseconds} ms with {nodes.Count} nodes for {geometryCount} geometries.");
            
            // Print tree
            int depth = 0;

            void PrintNode(int index, int currentDepth)
            {
                if (index == -1) return;
                if (currentDepth > depth) depth = currentDepth;
                var node = nodes[index];

                if (node.IsLeaf)
                {
                    Console.WriteLine(
                        $"{new string(' ', currentDepth * 2)} Leaf Node {index}: Geo [{node.Start}, {node.End}), " +
                        $"Bounds Min{node.Bounds.Min}, Max{node.Bounds.Max}");
                }
                else
                {
                    Console.WriteLine(
                        $"{new string(' ', currentDepth * 2)} Node {index}: Geos [{node.Start}, {node.End}), " +
                        $"Bounds Min{node.Bounds.Min}, Max{node.Bounds.Max}");
                }
                
                PrintNode(node.LeftChild, currentDepth + 1);
                PrintNode(node.RightChild, currentDepth + 1);
                
            }
            
            PrintNode(0, 0);
            Console.WriteLine($"TLAS Depth: {depth}");
        }
        return nodes.ToArray();
    }

    private static int BuildScene(Geometry[] geometries, int start, int end, int minGeos, int maxDepth,
        List<BVHNode> nodes)
    {
        var bbox = ComputeBoundingBox(geometries, start, end);
        int nodeIndex = nodes.Count;

        BVHNode node = new BVHNode
        {
            Bounds = bbox,
            Start = start,
            End = end,
            LeftChild = -1,
            RightChild = -1
        };
        nodes.Add(node);

        int geoCount = end - start;
        if (geoCount <= minGeos || maxDepth <= 0)
            return nodeIndex;

        int axis = bbox.LargestAxis();
        float split = 0.5f * (bbox.Min[axis] + bbox.Max[axis]);
        int mid = PartitionGeometries(geometries, start, end, axis, split);

        // Prevent empty partitions — fallback to median
        if (mid == start || mid == end)
            mid = start + (end - start) / 2;

        // build children recursively
        int leftChild = BuildScene(geometries, start, mid, minGeos, maxDepth - 1, nodes);
        int rightChild = BuildScene(geometries, mid, end, minGeos, maxDepth - 1, nodes);

        // link
        BVHNode n = nodes[nodeIndex];
        n.LeftChild = leftChild;
        n.RightChild = rightChild;
        nodes[nodeIndex] = n;

        return nodeIndex;
    }
    private static int PartitionGeometries(Geometry[] geometries, int start, int end, int axis, float split)
    {
        int i = start;
        int j = end - 1;

        while (i <= j)
        {
            while (i <= j && geometries[i].Centroid[axis] <= split) i++;
            while (i <= j && geometries[j].Centroid[axis] > split) j--;
            if (i < j)
            {
                (geometries[i], geometries[j]) = (geometries[j], geometries[i]);
                i++;
                j--;
            }
        }

        if (i == start || i == end)
            i = start + (end - start) / 2;

        return i;
    }

    private static BoundingBox ComputeBoundingBox(Geometry[] geometries, int start, int end)
    {
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);

        for (int i = start; i < end; i++)
        {
            ref var geo = ref geometries[i];
            min = Vector3.Min(min, geo.Bounds.Min);
            max = Vector3.Max(max, geo.Bounds.Max);
        }

        return new BoundingBox(min, max);
    }

    #endregion
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BoundingBox ComputeBoundingBox(Triangle[] triangles, int start, int end)
    {
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);

        for (int i = start; i < end; i++)
        {
            ref var tri = ref triangles[i];
            min = Vector3.Min(min, Vector3.Min(tri.V0, Vector3.Min(tri.V1, tri.V2)));
            max = Vector3.Max(max, Vector3.Max(tri.V0, Vector3.Max(tri.V1, tri.V2)));
        }

        return new BoundingBox(min, max);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LargestAxis(this BoundingBox box)
    {
        Vector3 size = box.Max - box.Min;
        if (size.X > size.Y && size.X > size.Z) return 0;
        if (size.Y > size.Z) return 1;
        return 2;
    }
}
