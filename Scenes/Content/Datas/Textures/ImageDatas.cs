using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.IO.SceneLoaders.Converters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Raytracer.Scenes.Content.Datas.Textures;

public class ImageDatas
{
    [JsonConverter(typeof(SingleOrListConverter<ImageData>))]
    public List<ImageData> Image;
    
    private List<RuntimeImageData> _loadedImages;

    public override string ToString()
    {
        var imagesInfo = new System.Text.StringBuilder();
        if (Image != null)
        {
            foreach (var image in Image)
            {
                imagesInfo.AppendLine(image.ToString());
            }
        }
        return $"Images:\n{imagesInfo}";
    }
    
    public RuntimeImageData GetImageData(int id)
    {
        if (_loadedImages == null) LoadImages();

        for (int i = 0; i < Image.Count; i++)
        {
            if (Image[i].Id == id)
            {
                return _loadedImages![i];
            }
        }

        throw new KeyNotFoundException($"Image with Id {id} not found.");
    }

    private void LoadImages()
    {
        _loadedImages = new List<RuntimeImageData>();
        foreach (var data in Image)
        {
            var path = Params.GetFilePathInSceneDir(data.Path);
            if (Path.GetExtension(path).Equals(".exr", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(path).Equals(".hdr", StringComparison.OrdinalIgnoreCase))
            {
                TinyEXR.Exr.LoadEXR(path, out var pixelData, out var width, out var height);
                var pixels = new List<Vector3>(width * height);
                for (int i = 0; i < pixelData.Length; i += 4)
                {                    
                    pixels.Add(new Vector3(pixelData[i], pixelData[i + 1], pixelData[i + 2]));
                }
                var runtimeImageData = new RuntimeImageData 
                {
                    Width = width,
                    Height = height,
                    Pixels = pixels
                };
                _loadedImages.Add(runtimeImageData);
            }
            else
            {
                var image = SixLabors.ImageSharp.Image.Load<Rgb24>(path);
                var pixelData = new List<Vector3>(image.Width * image.Height);
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image[x, y];
                        pixelData.Add(new Vector3(pixel.R, pixel.G, pixel.B));
                    }
                }
                var runtimeImageData = new RuntimeImageData
                {
                    Width = image.Width,
                    Height = image.Height,
                    Pixels = pixelData
                };
                _loadedImages.Add(runtimeImageData);
            }
        }
    }
}

public struct ImageData
{
    [JsonPropertyName("_data")] public string Path;
    [JsonPropertyName("_id")] public int Id;

    public override string ToString()
    {
        return $"Image Id: {Id}, Path: {Path}";
    }
}

public struct RuntimeImageData
{
    public int Width;
    public int Height;
    public List<Vector3> Pixels;
    
    public Vector3 GetPixel(int x, int y)
    {
        return Pixels[y * Width + x];
    }
}