namespace Raytracer.Scenes.Content.Datas.Textures;

public static class TextureExtensions
{
    public static TextureType ToTextureType(this string type)
    {
        return type.ToLower() switch
        {
            "image" => TextureType.Image,
            "perlin" => TextureType.Perlin,
            "checkerboard" => TextureType.Checkerboard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported texture type: {type}")
        };
    }
    
    public static DecalType ToDecalType(this string decalType)
    {
        return decalType.ToLower() switch
        {
            "replace_kd" => DecalType.ReplaceKD,
            "blend_kd" => DecalType.BlendKD,
            "replace_ks" => DecalType.ReplaceKS,
            "replace_background" => DecalType.ReplaceBackground,
            "replace_normal" => DecalType.ReplaceNormal,
            "bump_normal" => DecalType.BumpNormal,
            "replace_all" => DecalType.ReplaceAll,
            _ => throw new ArgumentOutOfRangeException(nameof(decalType), $"Unsupported decal type: {decalType}")
        };
    }
    
    public static InterpolationType ToInterpolationType(this string interpolationType)
    {
        if (string.IsNullOrWhiteSpace(interpolationType)) return InterpolationType.Nearest;
        return interpolationType.ToLower() switch
        {
            "nearest" => InterpolationType.Nearest,
            "bilinear" => InterpolationType.Bilinear,
            "trilinear" => InterpolationType.Trilinear,
            _ => throw new ArgumentOutOfRangeException(nameof(interpolationType), $"Unsupported interpolation type: {interpolationType}")
        };
    }
    
    public static NoiseConversionType ToNoiseConversionType(this string noiseConversionType)
    {
        return noiseConversionType.ToLower() switch
        {
            "absval" => NoiseConversionType.Absolute,
            "linear" => NoiseConversionType.Linear,
            _ => throw new ArgumentOutOfRangeException(nameof(noiseConversionType), $"Unsupported noise conversion type: {noiseConversionType}")
        };
    }
}

public enum TextureType
{
    Image,
    Perlin,
    Checkerboard
}

public enum DecalType
{
    ReplaceKD,
    BlendKD,
    ReplaceKS,
    ReplaceBackground,
    ReplaceNormal,
    BumpNormal,
    ReplaceAll
}

public enum InterpolationType
{
    None,
    Nearest,
    Bilinear,
    Trilinear
}

public enum NoiseConversionType
{
    Absolute,
    Linear
}