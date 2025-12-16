using System.Numerics;
using Raytracer.Rendering.Sampling;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime.Textures.Procedural;

public static class Perlin
{
    private static readonly int[] p = {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,
        20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,
        230,220,105,92,41,55,46,245,40,244,102,143,54, 65,25,63,161,1,216,80,73,209,76,132,187,208, 89,
        18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,72,56,249,160,137,
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,
        20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,
        230,220,105,92,41,55,46,245,40,244,102,143,54, 65,25,63,161,1,216,80,73,209,76,132,187,208, 89,
        18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,72,56,249,160,137
    };
    
    private static readonly int[] perm = new int[512];

    static Perlin()
    {
        for(int i = 0; i < 512; i++) 
        {
            perm[i] = p[i % 256];
        }
    }

    public static float Noise(float x, float y)
    {
        int X = (int)MathF.Floor(x) & 255;
        int Y = (int)MathF.Floor(y) & 255;
    
        x -= MathF.Floor(x);
        y -= MathF.Floor(y);
    
        float u = Fade(x);
        float v = Fade(y);
    
        int A = perm[X] + Y;
        int B = perm[X + 1] + Y;

        Vector2 uv = new Vector2(u, v);
        float valAA = Gradient(perm[A], x, y);
        float valBA = Gradient(perm[B], x - 1, y);
        float valAB = Gradient(perm[A + 1], x, y - 1);
        float valBB = Gradient(perm[B + 1], x - 1, y - 1);

        float lerpX1 = MathUtility.Lerp(valAA, valBA, u);
        float lerpX2 = MathUtility.Lerp(valAB, valBB, u);
        float result = MathUtility.Lerp(lerpX1, lerpX2, v);

        return result;
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Gradient(int hash, float x, float y)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : h == 12 || h == 14 ? x : 0;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}