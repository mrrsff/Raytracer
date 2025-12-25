using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Textures;

public struct TextureInfo
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public string Type;
    public int ImageId;
    public string Interpolation;
    public string DecalMode;
    public int Normalizer;
    public float BumpFactor;
    public float NoiseScale;
    public string NoiseConversion;
    public int NumOctaves;
    public float Scale;
    public float Offset;
    public float[] BlackColor;
    public float[] WhiteColor;
    
}