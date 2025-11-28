using System.Numerics;
using Raytracer.IO.ImageSavers;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;

namespace Raytracer.Rendering.Debug;

public class DrawLine(Vector3 From, Vector3 To, Vector3 color) : IDebugDrawCommand
{
    public float Thickness { get; set; } = 2.0f;
    public Vector3 Color { get; set; } = color;

    public void Draw(Camera camera, ImageBuffer buffer)
    {
        Vector2 p0 = DebugRenderer.ProjectToImagePlane(camera, From);
        Vector2 p1 = DebugRenderer.ProjectToImagePlane(camera, To);

        // Skip segments that are not projectable (behind / off-plane)
        if (p0.X < 0f || p0.Y < 0f || p1.X < 0f || p1.Y < 0f) return;

        int x0 = (int)MathF.Round(p0.X);
        int y0 = (int)MathF.Round(p0.Y);
        int x1 = (int)MathF.Round(p1.X);
        int y1 = (int)MathF.Round(p1.Y);

        int w = buffer.Width, h = buffer.Height;

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
            if (x0 < w && y0 < h)
            {
                // add thickness by drawing a square around the pixel
                int halfThickness = (int)(Thickness / 2);
                for (int ty = -halfThickness; ty <= halfThickness; ty++)
                {
                    for (int tx = -halfThickness; tx <= halfThickness; tx++)
                    {
                        int drawX = x0 + tx;
                        int drawY = y0 + ty;
                        if (drawX >= 0 && drawX < w && drawY >= 0 && drawY < h)
                        {
                            buffer.SetPixel(drawX, drawY, color);
                        }
                    }
                }
            }

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