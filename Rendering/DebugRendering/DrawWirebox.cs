using System.Numerics;
using Raytracer.IO.ImageSavers;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.DebugRendering;

public class DrawWirebox(Vector3 min, Vector3 max, Vector3 color) : IDebugDrawCommand
{
    public Vector3 Color { get; set; } = color;

    public void Draw(Camera camera, ImageBuffer buffer)
    {
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(min.X, min.Y, min.Z);
        corners[1] = new Vector3(max.X, min.Y, min.Z);
        corners[2] = new Vector3(max.X, max.Y, min.Z);
        corners[3] = new Vector3(min.X, max.Y, min.Z);
        corners[4] = new Vector3(min.X, min.Y, max.Z);
        corners[5] = new Vector3(max.X, min.Y, max.Z);
        corners[6] = new Vector3(max.X, max.Y, max.Z);
        corners[7] = new Vector3(min.X, max.Y, max.Z);

        int[,] edges = new int[,]
        {
            {0, 1}, {1, 2}, {2, 3}, {3, 0},
            {4, 5}, {5, 6}, {6, 7}, {7, 4},
            {0, 4}, {1, 5}, {2, 6}, {3, 7}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            Vector3 from = corners[edges[i, 0]];
            Vector3 to = corners[edges[i, 1]];
            var line = new DrawLine(from, to, Color);
            line.Draw(camera, buffer);
        }
    }
}