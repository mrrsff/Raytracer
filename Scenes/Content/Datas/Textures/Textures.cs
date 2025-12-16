using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.SceneLoaders.Converters;

namespace Raytracer.Scenes.Content.Datas.Textures;

public class Textures
{
    public ImageDatas Images;
    [JsonConverter(typeof(SingleOrListConverter<TextureInfo>))]
    public List<TextureInfo> TextureMap;

    public override string ToString()
    {
        var textureMaps = new StringBuilder();
        if (TextureMap != null)
        {
            foreach (var texture in TextureMap)
            {
                textureMaps.AppendLine(texture.ToString());
            }
        }
        return $"Textures:\n{Images}\n{textureMaps}";
    }
}