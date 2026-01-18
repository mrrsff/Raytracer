namespace Raytracer.IO.Images;

internal static class ExrSaver
{
    public static string SaveExr(string dirPath, ImageBuffer image)
    {
        int width = image.Width;
        int height = image.Height;

        float[] rgb = image.ToFloatRgbBuffer();

        string path = Path.Combine(dirPath, image.OutputName);

        var result = TinyEXR.Exr.SaveEXR(
            rgb,
            width,
            height,
            3,      // RGB
            false,
            path
        );

        if (result != TinyEXR.ResultCode.Success)
            throw new Exception($"TinyEXR failed with error code {result}");

        return path;
    }
    
}