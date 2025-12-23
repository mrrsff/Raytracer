using System.Diagnostics;

namespace Raytracer.IO.Images;

public static class MediaSaver
{
    public static string SaveImage(string dirPath, ImageBuffer image)
    {
        return IsExr(image.OutputName) ? ExrSaver.SaveExr(dirPath, image) : ImageSaver.SaveImage(dirPath, image);
    }

    private static bool IsExr(string name)
    {
        return name.ToLower().EndsWith(".exr");
    }
    public static string SaveBitmap(byte[] imageData, int width, int height, string dirPath, string imageName) => ImageSaver.SaveBitmap(imageData, width, height, dirPath, imageName);
    public static void SaveGIF(string outputDir, List<string> imagePaths, int fps) => GifSaver.SaveGIF(outputDir, imagePaths, fps);

}