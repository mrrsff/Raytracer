using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public abstract class TonemapOperator
{
    protected TMOOptions Options;
    protected float Saturation;
    protected float Gamma;

    protected TonemapOperator(TMOOptions options)
    {
        Options = options;
    }

    public virtual void Prepare(ImageBuffer image) { }
    public abstract Vector3 Tonemap(ImageBuffer image, int x, int y);
    public static float Luminance(in Vector3 color)
    {
        return 0.2126f * color.X +
               0.7152f * color.Y +
               0.0722f * color.Z;
    }
}