using Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;
using Raytracer.Scenes.Content.Datas.CameraData;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Rendering.CPU.Tonemaps;

public static class TonemapUtility
{
    public static TonemapOperator CreateTonemapFunction(TonemapData tonemapData)
    {
        return tonemapData.TMO switch
        {
            TMOType.Photographic => new PhotographicOperator(tonemapData.TMOOptions),
            TMOType.ACES => new AcesOperator(tonemapData.TMOOptions),
            TMOType.Filmic => new FilmicOperator(tonemapData.TMOOptions),
            TMOType.None => new NoneTonemapOperator(tonemapData.TMOOptions),
            _ => throw new NotImplementedException($"Tonemap type {tonemapData.TMO} is not implemented.")
        };
    }
    
    public static Tonemapper CreateTonemapFromData(TonemapData tonemapData)
    {
        TonemapOperator tonemapOperator = CreateTonemapFunction(tonemapData);
        
        return new Tonemapper(tonemapOperator, tonemapData);
    }
}