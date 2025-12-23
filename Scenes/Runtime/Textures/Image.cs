using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime.Textures;

public class Image : Texture
{
    public override TextureType TextureType => TextureType.Image;

    private readonly int Width;
    private readonly int Height;
    private readonly List<Mipmap> _mipmaps = [];
    
    public Image(TextureInfo textureInfo, ImageDatas imageDatas) : base(textureInfo)
    {
        // BumpFactor *= 0.0045f; // bump_mapping_transformed
        // BumpFactor *= 0.014f; // sphere_bump_mapping
        // BumpFactor *= 0.1f; // mytap
        if (BumpFactor >= 1f) BumpFactor *= 0.1f;
        
        var image = imageDatas.GetImageData(textureInfo.ImageId);
        var normalizer = textureInfo.Normalizer != 0 ? textureInfo.Normalizer : 255f;

        Width = image.Width;
        Height = image.Height;
        var pixels = new Vector3[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var pixel = image[x, y];
                Vector3 color = new Vector3(pixel.R, pixel.G, pixel.B) / normalizer;
                pixels[y * Width + x] = color; // [0, 1]
            }
        }

        int maxLevels = 1;
        if (InterpolationType == InterpolationType.Trilinear)
        {
            maxLevels = (int)MathF.Floor(MathF.Log2(MathF.Max(Width, Height))) + 1;
            maxLevels = Math.Max(1, maxLevels);
        }
        
        for (int level = 0; level < maxLevels; level++)
        {
            var mipmapLevel = Mipmap.Create(level, Width, Height, pixels);
            _mipmaps.Add(mipmapLevel);
        }
        
        du = new Vector2(1f / Width, 0f);
        dv = new Vector2(0f, 1f / Height);
        // SaveMipmaps();
    }

    public override Vector3 Sample(IntersectionInfo info)
    {
        Vector2 uv = info.GetUVCoordinates(false);
        if (InterpolationType == InterpolationType.Trilinear)
        {
            var mipLevel = Mipmap.ComputeMipLevel(info, Width, Height);
            if (Debug.RenderMipLevels) return new Vector3(mipLevel / (_mipmaps.Count - 1));
            return TrilinearSample(uv, mipLevel);
        }
        return SampleFromUV(uv);
    }

    public override Vector3 SampleFromUV(Vector2 uv)
    {
        Vector3 sample;
        switch (InterpolationType)
        {
            case InterpolationType.Nearest:
            {
                sample = NearestNeighborSample(uv);
                break;
            }
            case InterpolationType.Bilinear:
                sample = BilinearSample(_mipmaps[0], uv);
                break;
            case InterpolationType.Trilinear:
            case InterpolationType.None:
            default:
                throw new NotImplementedException($"Interpolation type {InterpolationType} not implemented.");
        }

        return sample;
    }

    private readonly Vector2 du;
    private readonly Vector2 dv;
    public void SampleHeightDerivatives(Vector2 uv, out float dhdu, out float dhdv)
    {
        float h = SampleHeight(uv);
        float hu = SampleHeight(uv + du);
        float hv = SampleHeight(uv + dv);

        dhdu = (hu - h) * Width;
        dhdv = (hv - h) * Height;
    }

    private float SampleHeight(Vector2 uv) // [0, 1]
    {
        Vector3 sample = SampleFromUV(uv); 
        return (sample.X + sample.Y + sample.Z) / 3f;
    }

    private Vector3 NearestNeighborSample(Vector2 uv)
    {
        uv.X -= MathF.Floor(uv.X);
        uv.Y -= MathF.Floor(uv.Y);
        
        int x = (int)MathF.Round(uv.X * (Width  - 1));
        int y = (int)MathF.Round(uv.Y * (Height - 1));
        return _mipmaps[0].Pixels[y * Width + x];
    }
    private static Vector3 BilinearSample(Mipmap mip, Vector2 uv)
    {
        uv.X -= MathF.Floor(uv.X);
        uv.Y -= MathF.Floor(uv.Y);
        
        float x = uv.X * (mip.Width  - 1);
        float y = uv.Y * (mip.Height - 1);

        int x0 = (int)MathF.Floor(x);
        int y0 = (int)MathF.Floor(y);

        float tx = x - x0;
        float ty = y - y0;
        
        int x1 = Math.Min(x0 + 1, mip.Width - 1);
        int y1 = Math.Min(y0 + 1, mip.Height - 1);
        
        Vector3 c00 = mip.Pixels[y0 * mip.Width + x0];
        Vector3 c10 = mip.Pixels[y0 * mip.Width + x1];
        Vector3 c01 = mip.Pixels[y1 * mip.Width + x0];
        Vector3 c11 = mip.Pixels[y1 * mip.Width + x1];

        Vector3 c0 = Vector3.Lerp(c00, c10, tx);
        Vector3 c1 = Vector3.Lerp(c01, c11, tx);
        return Vector3.Lerp(c0, c1, ty);
    }
    
    private Vector3 TrilinearSample(Vector2 uv, float mipLevel)
    {
        int level0 = Math.Clamp((int)MathF.Floor(mipLevel), 0, _mipmaps.Count - 1);
        int level1 = Math.Clamp(level0 + 1, 0, _mipmaps.Count - 1);
        
        float t = mipLevel - level0;
        
        // Debug.Log($"Trilinear Sample: mipLevel={mipLevel}, level0={level0}, level1={level1}, t={t}");
        
        Mipmap mip0 = _mipmaps[level0];
        Mipmap mip1 = _mipmaps[level1];

        Vector3 c0 = BilinearSample(mip0, uv);
        Vector3 c1 = BilinearSample(mip1, uv);
    
        return Vector3.Lerp(c0, c1, t);
    }
    
    public void SaveMipmaps()
    {
        for (int level = 0; level < _mipmaps.Count; level++)
        {
            var mip = _mipmaps[level];
            var bytes = new byte[mip.Width * mip.Height * 3];
            for (int y = 0; y < mip.Height; y++)
            {
                for (int x = 0; x < mip.Width; x++)
                {
                    var color = mip.Pixels[y * mip.Width + x];
                    if (color.X > 1f || color.Y > 1f || color.Z > 1f)
                        color = ColorUtility.Normalize(color);
                    int r = (int)(color.X * 255);
                    int g = (int)(color.Y * 255);
                    int b = (int)(color.Z * 255);
                    int index = (y * mip.Width + x) * 3;
                    bytes[index + 0] = (byte)r;
                    bytes[index + 1] = (byte)g;
                    bytes[index + 2] = (byte)b;
                }
            }
            string path = $"texture_{Id}_mip_{level}.png";
            MediaSaver.SaveBitmap(bytes, mip.Width, mip.Height, Params.OutputDirectory ?? "", Path.GetFileName(path));
        }
    }
}