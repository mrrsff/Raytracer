using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace Raytracer.IO.Images;

internal static class GifSaver
{
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