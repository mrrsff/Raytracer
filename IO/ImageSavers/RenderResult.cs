using System;
using System.Numerics;
using Raytracer.Core;

namespace Raytracer.IO.ImageSavers;

public class RenderResult
{
    public int Width;
    public int Height;
    public Vector3[] Pixels;
    public string OutputName = "output.png";
    
    public RenderResult(Resolution resolution, Vector3 backgroundColor) 
        : this(resolution.Width, resolution.Height, backgroundColor) { }
    public RenderResult(int width, int height, Vector3 backgroundColor)
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
}