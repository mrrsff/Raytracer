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
        
        image.CopyPixelDataTo(imageBuffer.ToByteBuffer());

        image.Save(path);
    }
}