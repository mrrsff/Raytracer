using System.Text;
using System.Text.Json.Serialization;
using Raytracer.IO.SceneLoaders.Converters;

namespace Raytracer.Scenes.Content.Datas.CameraData;

[Serializable]
public struct TonemapData
{
    public TMOType TMO;
    public TMOOptions TMOOptions;
    public float Saturation;
    public float Gamma;
    public string Extension;

    public override string ToString() =>
        new StringBuilder().Append("TonemapData(TMO: ")
            .Append(TMO)
            .Append(", Saturation: ")
            .Append(Saturation)
            .Append(", Gamma: ")
            .Append(Gamma)
            .Append(", Extension: ")
            .Append(Extension)
            .Append(')').ToString();
}

public enum TMOType
{
    Photographic = 0, // Reinhard
    ACES = 1,
    Filmic = 2,
    None = 3
}

[Serializable]
public struct TMOOptions
{
    public float[] Params;
}