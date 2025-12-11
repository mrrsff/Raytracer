using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.DebugRendering;

public class DrawTriangle : IDebugDrawCommand
{
    public Vector3 V0 { get; set; }
    public Vector3 V1 { get; set; }
    public Vector3 V2 { get; set; }
    public Vector3 Color { get; set; }
    
    public DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 color)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Color = color;
    }
    public void Draw(Camera camera, ImageBuffer buffer)
    {
        var screenV0 = DebugRenderer.ProjectToImagePlane(camera, V0);
        var screenV1 = DebugRenderer.ProjectToImagePlane(camera, V1);
        var screenV2 = DebugRenderer.ProjectToImagePlane(camera, V2);
        
        int x0 = (int)MathF.Round(screenV0.X);
        int y0 = (int)MathF.Round(screenV0.Y);
        int x1 = (int)MathF.Round(screenV1.X);
        int y1 = (int)MathF.Round(screenV1.Y);
        int x2 = (int)MathF.Round(screenV2.X);
        int y2 = (int)MathF.Round(screenV2.Y);

        int w = buffer.Width;
        int h = buffer.Height;

        int minX = Math.Clamp(Math.Min(x0, Math.Min(x1, x2)), 0, w - 1);
        int maxX = Math.Clamp(Math.Max(x0, Math.Max(x1, x2)), 0, w - 1);
        int minY = Math.Clamp(Math.Min(y0, Math.Min(y1, y2)), 0, h - 1);
        int maxY = Math.Clamp(Math.Max(y0, Math.Max(y1, y2)), 0, h - 1);

        float denom = (y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2);
        if (MathF.Abs(denom) < 1e-6f) return;

        float invDenom = 1f / denom;

        for (int py = minY; py <= maxY; py++)
        {
            for (int px = minX; px <= maxX; px++)
            {
                // Compute barycentric coordinates (u,v,w)
                float u = ((y1 - y2) * (px - x2) + (x2 - x1) * (py - y2)) * invDenom;
                float v = ((y2 - y0) * (px - x2) + (x0 - x2) * (py - y2)) * invDenom;
                float t = 1f - u - v;

                // If inside triangle or on edges
                if (u >= 0f && v >= 0f && t >= 0f)
                {
                    buffer.SetPixel(px, py, Color);
                }
            }
        }
    }
}