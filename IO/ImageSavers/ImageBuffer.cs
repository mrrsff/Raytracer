using System;
using System.Numerics;
using Raytracer.Core;
using Raytracer.Utility;

namespace Raytracer.IO.ImageSavers;

public class ImageBuffer
{
    public int Width;
    public int Height;
    public Vector3[] Pixels;
    public string OutputName = "output.png";

    public ImageBuffer(Resolution resolution, Vector3 backgroundColor)
        : this(resolution.Width, resolution.Height, backgroundColor)
    {
    }

    public ImageBuffer(int width, int height, Vector3 backgroundColor)
    {
        Width = width;
        Height = height;

        Pixels = new Vector3[width * height];
        
        for (int i = 0; i < Pixels.Length; i++)
        {
            Pixels[i] = backgroundColor;
        }
    }
    public void SetPixel(int x, int y, Vector3 color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        Pixels[y * Width + x] = color;
    }

    public Vector3 GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return default;
        return Pixels[y * Width + x];
    }

    public void SetData(Vector3[] data)
    {
        if (data.Length != Width * Height)
            throw new ArgumentException("Data length does not match image dimensions.");
        Pixels = data;
    }
    
    public byte[] ToByteBuffer()
    {
        int n = Width * Height;
        var result = new byte[n * 4];

        for (int i = 0; i < n; i++)
        {
            var c = Pixels[i];

            byte r = (byte)(Math.Clamp(c.X, 0f, 1f) * 255);
            byte g = (byte)(Math.Clamp(c.Y, 0f, 1f) * 255);
            byte b = (byte)(Math.Clamp(c.Z, 0f, 1f) * 255);

            int o = i * 4;
            result[o + 0] = r;
            result[o + 1] = g;
            result[o + 2] = b;
            result[o + 3] = 255;
        }

        return result;
    }
}