using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Raytracer.IO.Images;

internal static class ImageSaver
{
    public static string SaveImage(string dirPath, ImageBuffer image)
    {
        int width = image.Width;
        int height = image.Height;

        using var output = new Image<Rgb24>(width, height);

        var buffer = image.ToByteBuffer();

        int i = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte b = buffer[i++];
                byte g = buffer[i++];
                byte r = buffer[i++];
                i++;
                
                output[x, y] = new Rgb24(r, g, b);
            }
        }

        string path = Path.Combine(dirPath, image.OutputName);
        output.Save(path);

        return path;
    }
    
    public static string SaveBitmap(byte[] imageData, int width, int height, string dirPath, string imageName)
    {
        using var image = Image.LoadPixelData<Rgb24>(imageData, width, height);
        var path = Path.Combine(dirPath, imageName);
        image.Save(path);
        return path;
    }
}