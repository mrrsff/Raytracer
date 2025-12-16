using System.Numerics;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Scenes.Runtime.Textures;

public class CheckerboardTexture : Texture
{
    public override TextureType TextureType => TextureType.Checkerboard;
    public readonly float Scale;
    public readonly float Offset;
    public readonly Vector3 BlackColor;
    public readonly Vector3 WhiteColor;

    public CheckerboardTexture(TextureInfo textureInfo) : base(textureInfo)
    {
        Scale = textureInfo.Scale;
        Offset = textureInfo.Offset;
        if (textureInfo.BlackColor.Length != 3 || textureInfo.WhiteColor.Length != 3)
            throw new ArgumentException("BlackColor and WhiteColor must have exactly 3 components each.");
        BlackColor = new Vector3(textureInfo.BlackColor[0], textureInfo.BlackColor[1], textureInfo.BlackColor[2]);
        WhiteColor = new Vector3(textureInfo.WhiteColor[0], textureInfo.WhiteColor[1], textureInfo.WhiteColor[2]);
    }
    public override Vector3 Sample(IntersectionInfo info)
    {
        var pos = info.Point;
        bool x = ((int)MathF.Floor((pos.X + Offset) * Scale)) % 2 == 0;
        bool y = ((int)MathF.Floor((pos.Y + Offset) * Scale)) % 2 == 0;
        bool z = ((int)MathF.Floor((pos.Z + Offset) * Scale)) % 2 == 0;
        bool xorXY = x != y;
        return xorXY != z ? WhiteColor : BlackColor;
    }

    public override Vector3 SampleUV(Vector2 uv)
    {
        bool u = ((int)MathF.Floor((uv.X + Offset) * Scale)) % 2 == 0;
        bool v = ((int)MathF.Floor((uv.Y + Offset) * Scale)) % 2 == 0;
        bool xorUV = u != v;
        return xorUV ? WhiteColor : BlackColor;
    }
}