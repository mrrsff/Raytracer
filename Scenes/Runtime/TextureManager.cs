using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Scenes.Runtime.Textures;

namespace Raytracer.Scenes.Runtime;

public class TextureManager
{
    public List<Image> Images = new();
    public List<PerlinTexture> PerlinTextures = new();
    public List<CheckerboardTexture> CheckerboardTextures = new();
    
    public Dictionary<int, Texture> Textures = new();
    public void LoadTextures(SceneContent content)
    {
        if (content.Textures == null) return;
        
        foreach (var textureMap in content.Textures.TextureMap)
        {
            Texture texture = textureMap.Type switch
            {
                "image" => new Image(textureMap, content.Textures.Images),
                "perlin" => new PerlinTexture(textureMap),
                "checkerboard" => new CheckerboardTexture(textureMap),
                _ => throw new NotImplementedException($"Texture type {textureMap.Type} is not implemented.")
            };

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
    
    public T GetAs<T>(int id) where T : Texture
    {
        if (Textures.TryGetValue(id, out var texture))
        {
            if (texture is T typedTexture)
            {
                return typedTexture;
            }
            throw new InvalidCastException($"Texture with Id: {id} is not of type {typeof(T).Name}.");
        }
        throw new KeyNotFoundException($"Texture with Id: {id} not found.");
    }
    
    public Texture GetTexture(int id)
    {
        return Textures.TryGetValue(id, out var texture) ? texture : throw new KeyNotFoundException($"Texture with Id: {id} not found.");
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