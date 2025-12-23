using System;
using System.Numerics;
using Raytracer.Core;
using Raytracer.Utility;

namespace Raytracer.IO.Images;

public class ImageBuffer
{
    public int Width;
    public int Height;
    public string OutputName = "output.png";
    public Vector3[] Pixels;

    public ImageBuffer(Resolution resolution, Vector3 backgroundColor, string outputName)
        : this(resolution.Width, resolution.Height, backgroundColor)
    {
        OutputName = outputName;
    }

    private ImageBuffer(int width, int height, Vector3[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
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
    public ImageBuffer Normalize()
    {
        Vector3[] normalizedPixels = new Vector3[Pixels.Length];
        for (int i = 0; i < Pixels.Length; i++)
        {
            normalizedPixels[i] = ColorUtility.Normalize(Pixels[i]);
        }

        return new ImageBuffer(Width, Height, normalizedPixels);
    }
    private byte[] byteBuffer = null!;
    public byte[] ToByteBuffer()
    {
        int n = Width * Height;
        if (byteBuffer == null || byteBuffer.Length != n * 4)
        {
            byteBuffer = new byte[n * 4];
        }

        for (int i = 0; i < n; i++)
        {
            var c = Pixels[i];

            byte r = (byte)(Math.Clamp(c.X, 0f, 1f) * 255);
            byte g = (byte)(Math.Clamp(c.Y, 0f, 1f) * 255);
            byte b = (byte)(Math.Clamp(c.Z, 0f, 1f) * 255);
            
            int o = i * 4;
            byteBuffer[o + 0] = r;
            byteBuffer[o + 1] = g;
            byteBuffer[o + 2] = b;
            byteBuffer[o + 3] = 255;
        }
        return byteBuffer;
    }
    
    public float[] ToFloatRgbBuffer()
    {
        var data = new float[Width * Height * 3];
        int i = 0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var c = GetPixel(x, y); // Vector3 HDR color
                data[i++] = c.X;
                data[i++] = c.Y;
                data[i++] = c.Z;
            }
        }

        return data;
    }

    public ImageBuffer Clone()
    {
        Vector3[] pixelsCopy = new Vector3[Pixels.Length];
        Array.Copy(Pixels, pixelsCopy, Pixels.Length);
        return new ImageBuffer(Width, Height, pixelsCopy)
        {
            OutputName = OutputName
        };
    }
}