using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Scenes.Runtime.Textures;

namespace Raytracer.Scenes.Runtime;

public class TextureManager
{
    public List<Image> Images = new();
    public List<HDRImage> HDRImages = new();
    public List<PerlinTexture> PerlinTextures = new();
    public List<CheckerboardTexture> CheckerboardTextures = new();
    
    public Dictionary<int, Texture> Textures = new();
    public void LoadTextures(SceneContent content)
    {
        if (content.Textures == null) return;
        
        List<int> loadedImageIds = new();
        if (content.Textures.TextureMap != null)
        {
            foreach (var textureMap in content.Textures.TextureMap)
            {
                Texture texture = textureMap.Type switch
                {
                    "image" => new Image(textureMap, content.Textures.Images),
                    "perlin" => new PerlinTexture(textureMap),
                    "checkerboard" => new CheckerboardTexture(textureMap),
                    _ => throw new NotImplementedException($"Texture type {textureMap.Type} is not implemented.")
                };
                loadedImageIds.Add(textureMap.ImageId);

                Textures.TryAdd(textureMap.Id, texture);

                switch (texture)
                {
                    case Image img:
                        Images.Add(img);
                        break;
                    case PerlinTexture perlinTex:
                        PerlinTextures.Add(perlinTex);
                        break;
                    case CheckerboardTexture checkerTex:
                        CheckerboardTextures.Add(checkerTex);
                        break;
                }
            }
        }
        var imgDatas2 = content.Textures.Images;
        if (imgDatas2 == null) return;
        
        foreach (var imgData in imgDatas2.Image)
        {
            if (loadedImageIds.Contains(imgData.Id)) continue;
            var imageTexture = new HDRImage(imgData);
            Textures.TryAdd(imgData.Id, imageTexture);
            HDRImages.Add(imageTexture);
        }
    }
    public Texture GetTexture(int id)
    {
        if (!Textures.ContainsKey(id) && HDRImages.All(tex => tex.Id != id))
            throw new KeyNotFoundException($"Texture with ID {id} not found.");
        
        return Textures.TryGetValue(id, out var texture) ? texture : HDRImages.First(tex => tex.Id == id);
    }
    
    public bool TryGetBackgroundTexture(out Texture? texture)
    {
        foreach (var tex in Textures.Values.Where(tex => tex.DecalType == DecalType.ReplaceBackground))
        {
            texture = tex;
            return true;
        }
        texture = null;
        return false;
    }
}