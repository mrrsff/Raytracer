using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.RenderingDebug;

public class DrawPoint(Vector3 position, float size, Vector3 color) : IDebugDrawCommand
{
    public Vector3 Position = position;
    public float Size = size;
    public Vector3 Color { get; set; } = color;

    public void Draw(Camera camera, ImageBuffer buffer)
    {
        Vector2 pos = DebugRenderer.ProjectToImagePlane(camera, Position);
        
        // Skip points that are not projectable (behind / off-plane)
        if (pos.X < 0f || pos.Y < 0f) return;
        int xCenter = (int)MathF.Round(pos.X);
        int yCenter = (int)MathF.Round(pos.Y);
        int halfSize = (int)(Size / 2);
        for (int y = -halfSize; y <= halfSize; y++)
        {
            for (int x = -halfSize; x <= halfSize; x++)
            {
                int drawX = xCenter + x;
                int drawY = yCenter + y;
                if (x * x + y * y > halfSize * halfSize) continue;
                if (drawX >= 0 && drawX < buffer.Width && drawY >= 0 && drawY < buffer.Height)
                {
                    buffer.SetPixel(drawX, drawY, Color);
                }
            }
        }
        
    }
}