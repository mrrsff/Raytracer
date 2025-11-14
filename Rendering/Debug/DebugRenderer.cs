using System.Numerics;
using Raytracer.IO.ImageSavers;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Runtime.Meshes.BVH;

namespace Raytracer.Rendering.Debug;

public static class DebugRenderer
{
    private static readonly List<IDebugGeometry> geometries = new();
    public static void Add(IDebugGeometry g) => geometries.Add(g);
    public static IEnumerable<IDebugGeometry> GetAll() => geometries;
    public static void Clear() => geometries.Clear();

    public static void CollectBVH(BoundingVolumeHierarchy bvh, bool leafOnly)
    {
        for (int i = 0; i < bvh.NodeCount; i++)
        {
            ref var node = ref bvh.GetNode(i);
            if (leafOnly && !node.IsLeaf) continue;
            Add(new DebugBoundingBox(node.Bounds.Min, node.Bounds.Max, node.IsLeaf));
        }
    }

    public static void Rasterize(Camera camera, ImageBuffer imageBuffer)
    {
        foreach (var g in geometries)
        {
            if (g is DebugBoundingBox box)
            {
                DrawBoundingBoxWire(camera, imageBuffer, box.Min, box.Max, box.IsLeaf);
            }
        }
    }

    private static void DrawBoundingBoxWire(Camera camera, ImageBuffer img, Vector3 min, Vector3 max, bool leaf)
    {
        Vector3 color = leaf ? new(1, 0, 0) : new(0, 1, 0);

        // define 8 corners of the box
        ReadOnlySpan<Vector3> c =
        [
            new(min.X, min.Y, min.Z),
            new(max.X, min.Y, min.Z),
            new(max.X, max.Y, min.Z),
            new(min.X, max.Y, min.Z),
            new(min.X, min.Y, max.Z),
            new(max.X, min.Y, max.Z),
            new(max.X, max.Y, max.Z),
            new(min.X, max.Y, max.Z)
        ];

        // define edges as pairs of indices
        ReadOnlySpan<(int, int)> edges =
        [
            (0, 1), (1, 2), (2, 3), (3, 0), // front face
            (4, 5), (5, 6), (6, 7), (7, 4), // back face
            (0, 4), (1, 5), (2, 6), (3, 7) // connecting edges
        ];

        foreach (var (i0, i1) in edges)
        {
            DrawLine2D(camera, img, c[i0], c[i1], color);
        }
    }

    private static Vector2 ProjectToImagePlane(Camera camera, Vector3 point)
    {
        // camera basis is already set in InitializeCamera()
        Vector3 d = point - camera.Position;

        // Forward distance (depth along view direction)
        float z = Vector3.Dot(d, camera.Forward);
        if (z <= 0f) return new Vector2(-1f, -1f); // behind camera

        // Coordinates on the near plane (world-space) by similar triangles
        float u = (Vector3.Dot(d, camera.Right) / z) * camera.NearDistance;
        float v = (Vector3.Dot(d, camera.Up) / z) * camera.NearDistance;

        // Reject if outside the image plane extents
        float L = camera.NearPlane.Left;
        float R = camera.NearPlane.Right;
        float B = camera.NearPlane.Bottom;
        float T = camera.NearPlane.Top;

        if (u < L || u > R || v < B || v > T) return new Vector2(-1f, -1f);

        // Map to pixel coordinates
        float sx = (u - L) / (R - L); // [0,1]
        float sy = (T - v) / (T - B); // [0,1] (note Y down in image)
        float px = sx * camera.ImageResolution.Width;
        float py = sy * camera.ImageResolution.Height;

        return new Vector2(px, py);
    }

    private static void DrawLine2D(Camera camera, ImageBuffer img, Vector3 a, Vector3 b, Vector3 color)
    {
        Vector2 p0 = ProjectToImagePlane(camera, a);
        Vector2 p1 = ProjectToImagePlane(camera, b);

        // Skip segments that are not projectable (behind / off-plane)
        if (p0.X < 0f || p0.Y < 0f || p1.X < 0f || p1.Y < 0f) return;

        int x0 = (int)MathF.Round(p0.X);
        int y0 = (int)MathF.Round(p0.Y);
        int x1 = (int)MathF.Round(p1.X);
        int y1 = (int)MathF.Round(p1.Y);

        int w = img.Width, h = img.Height;

        // Cohen–Sutherland style trivial reject (both completely outside the same side)
        if ((x0 < 0 && x1 < 0) || (x0 >= w && x1 >= w) ||
            (y0 < 0 && y1 < 0) || (y0 >= h && y1 >= h)) return;

        // Bresenham
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if ((uint)x0 < (uint)w && (uint)y0 < (uint)h)
                img.SetPixel(x0, y0, color);

            if (x0 == x1 && y0 == y1) break;

            int e2 = err << 1;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}

public readonly struct DebugBoundingBox(Vector3 min, Vector3 max, bool isLeaf) : IDebugGeometry
{
    public Vector3 Min { get; } = min;
    public Vector3 Max { get; } = max;
    public bool IsLeaf { get; } = isLeaf;
}