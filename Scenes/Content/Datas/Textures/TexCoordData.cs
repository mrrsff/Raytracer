using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Textures;

public enum CoordType
{
    uv,
    uvw
}
public class TexCoordData
{
    [JsonPropertyName("_data")] public float[] Data;
    [JsonPropertyName("_type")] public CoordType Type;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Texture Coordinates:");
        sb.AppendLine($"Type: {Type}");
        sb.AppendLine("Data:");
        for (int i = 0; i < Data.Length; i += (Type == CoordType.uv ? 2 : 3))
        {
            if (Type == CoordType.uv)
            {
                sb.AppendLine($"  ({Data[i]}, {Data[i + 1]})");
            }
            else // uvw
            {
                sb.AppendLine($"  ({Data[i]}, {Data[i + 1]}, {Data[i + 2]})");
            }
        }
        return sb.ToString();
    }
    
    public Vector2 At(int index)
    {
        if (Type != CoordType.uv)
            throw new InvalidOperationException("TexCoordData does not contain UV coordinates.");
        index -= 1;
        int i = index * 2;
        if (i < 0 || i + 1 >= Data.Length)
            return Vector2.Zero;
        return new Vector2(Data[i], Data[i + 1]);
    }
}