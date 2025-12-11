using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.ImageSavers;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.DebugRendering;

public static class DebugRenderer
{
    private static readonly List<IDebugDrawCommand> drawCommands = new();
    public static void Add(IDebugDrawCommand cmd) => drawCommands.Add(cmd);

    public static void Rasterize(Camera camera, ImageBuffer imageBuffer)
    {
        if (!Debug.EnableDebugRendering) return; 
        
        foreach (var cmd in drawCommands)
        {
            cmd.Draw(camera, imageBuffer);
        }
    }

    public static Vector2 ProjectToImagePlane(Camera camera, Vector3 point)
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
}