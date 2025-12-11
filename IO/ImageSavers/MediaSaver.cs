using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Raytracer.IO.ImageSavers;

public abstract class MediaSaver
{
    public static string SaveImage(string dirPath, ImageBuffer imageBuffer)
    {
        var width = imageBuffer.Width;
        var height = imageBuffer.Height;
        using var image = new Image<Rgba32>(width, height);
        var buffer = imageBuffer.ToByteBuffer().AsSpan();
        
        // image.CopyPixelDataTo(buffer);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                byte r = buffer[index];
                byte g = buffer[index + 1];
                byte b = buffer[index + 2];
                byte a = buffer[index + 3];
                image[x, y] = new Rgba32(r, g, b, a);
            }
        }

        var combinedPath = Path.Combine(dirPath, imageBuffer.OutputName);
        image.Save(combinedPath);

        return combinedPath;
    }
    public static void SaveGIF(string outputDir, List<string> imagePaths, int fps)
    {
        var imageName = imagePaths[0].Split(Path.DirectorySeparatorChar).Last().Split('_').FirstOrDefault();
        
        var path = Path.Combine(outputDir, imageName ?? "output");
        path = Path.ChangeExtension(path, ".gif");
        if (File.Exists(path))
            File.Delete(path);
        
        Console.WriteLine($"[progress] gif_encoding_begin frames={imagePaths.Count} fps={fps} target={Path.GetFileName(path)}");

        // Frame delay in 1/100ths of a second for GIF metadata
        int frameDelay = (int)Math.Round(100.0 / fps);

        var baseImage = Image.Load<Rgba32>(imagePaths[0]);
        using var gif = new Image<Rgba32>(baseImage.Width, baseImage.Height);

        for (var i = imagePaths.Count - 1; i >= 0; i--) // reverse order for correct playback
        {
            var imgPath = imagePaths[i];
            using var frameImage = Image.Load<Rgba32>(imgPath);

            var frame = frameImage.Frames.RootFrame;
            var meta = frame.Metadata.GetGifMetadata();
            meta.FrameDelay = frameDelay;
            meta.DisposalMethod = GifDisposalMethod.RestoreToBackground;

            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
        }

        gif.Frames.RemoveFrame(0); // remove initial empty frame
        gif.SaveAsGif(path);

        Console.WriteLine($"[progress] gif_saved frames={imagePaths.Count} name={Path.GetFileName(path)}");
    }

}