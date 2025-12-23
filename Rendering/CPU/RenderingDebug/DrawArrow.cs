using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.RenderingDebug;

public class DrawArrow : IDebugDrawCommand
{
    public Vector3 Start { get; set; }
    public Vector3 End { get; set; }
    public float HeadSize { get; set; }
    public Vector3 Color { get; set; }
    public DrawArrow(Vector3 start, Vector3 end, Vector3 color, float headSize = .5f)
    {
        Start = start;
        End = end;
        Color = color;
        HeadSize = headSize;
    }

    public void Draw(Camera camera, ImageBuffer buffer)
    {
        Vector3 dir = Vector3.Normalize(End - Start);

        Vector3 camUp = Vector3.Normalize(camera.Up);
        Vector3 right = Vector3.Cross(dir, camUp);
        right = Vector3.Normalize(right.LengthSquared() < 1e-6f ?
            Vector3.Cross(dir, camera.Forward) : right);

        Vector3 top  = End + dir * HeadSize;
        Vector3 left = End + right * (HeadSize * 0.5f);
        Vector3 rightPt = End - right * (HeadSize * 0.5f);
        
        new DrawLine(Start, End, Color).Draw(camera, buffer);

        new DrawTriangle(top, left, rightPt, Color).Draw(camera, buffer);
    }
}