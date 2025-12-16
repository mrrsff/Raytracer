using System.Numerics;
using Raytracer.Core;

namespace Raytracer.Scenes.Runtime.Textures;

internal struct Mipmap
{
    public int Width;
    public int Height;
    public Vector3[] Pixels;

    public static Mipmap Create(int level, int originalWidth, int originalHeight, Vector3[] originalPixels)
    {
        int mipWidth = Math.Max(1, originalWidth >> level);
        int mipHeight = Math.Max(1, originalHeight >> level);
        Vector3[] mipPixels = new Vector3[mipWidth * mipHeight];
        int mult = 1 << level;
        for (int y = 0; y < mipHeight; y++)
        {
            for (int x = 0; x < mipWidth; x++)
            {
                int srcX = Math.Min(x * mult, originalWidth - 1);
                int srcY = Math.Min(y * mult, originalHeight - 1);

                mipPixels[y * mipWidth + x] = originalPixels[srcY * originalWidth + srcX];
            }
        }

        return new Mipmap
        {
            Width = mipWidth,
            Height = mipHeight,
            Pixels = mipPixels
        };
    }
}