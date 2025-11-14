using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Raytracer.IO.ImageSavers;

public class ImageSaver
{
    public static void SaveImage(string path, ImageBuffer imageBuffer)
    {
        var width = imageBuffer.Width;
        var height = imageBuffer.Height;
        using var image = new Image<Rgba32>(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                var c = imageBuffer.Pixels[i] * 255f;
                image[x, y] = new Rgba32(
                    (byte)Math.Clamp(c.X, 0, 255),
                    (byte)Math.Clamp(c.Y, 0, 255),
                    (byte)Math.Clamp(c.Z, 0, 255),
                    255
                );
            }
        }

        image.Save(path);
    }
}