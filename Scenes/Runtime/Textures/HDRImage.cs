using System.Numerics;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Scenes.Runtime.Textures;

public class HDRImage : Texture
{
    public override TextureType TextureType => TextureType.None;
    
    private readonly float[] rgba;
    private readonly int width;
    private readonly int height;
    public HDRImage(ImageData data) : base(data.Id)
    {
        var path = Params.GetFilePathInSceneDir(data.Path);
        TinyEXR.Exr.LoadEXR(path, out rgba, out width, out height);
    }
    public override Vector3 Sample(IntersectionInfo info)
    {
        // no-op
        return Vector3.Zero; 
    }

    public override Vector3 SampleFromUV(Vector2 uv)
    {
        uv.X -= MathF.Floor(uv.X);
        uv.Y -= MathF.Floor(uv.Y);
        
        int x = (int)(uv.X * width);
        int y = (int)(uv.Y * height);
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        
        int index = (y * width + x) * 4;
        return new Vector3(rgba[index], rgba[index + 1], rgba[index + 2]);
    }
}