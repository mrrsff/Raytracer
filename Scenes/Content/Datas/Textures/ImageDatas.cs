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
    
    private List<Image<Rgb24>> _loadedImages;

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
    
    public Image<Rgb24> GetImageData(int id)
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
        _loadedImages = [];
        foreach (var data in Image)
        {
            var path = Params.GetFilePathInSceneDir(data.Path);
            var image = SixLabors.ImageSharp.Image.Load<Rgb24>(path);
            _loadedImages.Add(image);
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