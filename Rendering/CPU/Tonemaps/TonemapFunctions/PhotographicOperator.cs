using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class PhotographicOperator : TonemapOperator
{
    public PhotographicOperator(TMOOptions options) : base(options) { }

    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        Vector3 color = image.GetPixel(x, y);
        float L = Luminance(color);
        
        float Llocal = FindRegionScale(x, y);
        float L_zoned = alpha * (L / Llocal);
        
        float L_compressed;
        if (burnOutPercentage > 0)
        {
            float Lwhite2 = burnOutLuminance * burnOutLuminance;
            var burnOutMultiplier = 1.0f + (L_zoned) / Lwhite2;
            L_compressed = (L_zoned * burnOutMultiplier) / (1.0f + L_zoned);
        }
        else L_compressed = L_zoned / (1.0f + L_zoned);

        var result = color * L_compressed / L;
        
        return result;
    }

    private float FindRegionScale(int x, int y)
    {
        return logAverageLuminance;
    }
}