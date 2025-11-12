using System.Numerics;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas;

public class VertexData
{
    [JsonPropertyName("_data")] public float[] Positions;
    private Vector3[] _cachedVectors;

    public override string ToString()
    {
        return $"VertexData(Positions: [{string.Join(", ", Positions)}])";
    }

    public Vector3 At(int index)
    {
        index -= 1;
        var i = index * 3;
        if (i < 0 || i + 2 >= Positions.Length)
            throw new IndexOutOfRangeException(
                $"Index {index} is out of range for VertexData with {Positions.Length / 3} vertices.");

        if (_cachedVectors == null)
        {
            int count = Positions.Length / 3;
            _cachedVectors = new Vector3[count];
        }

        if (_cachedVectors[index] == Vector3.Zero)
        {
            _cachedVectors[index] = new Vector3(Positions[i], Positions[i + 1], Positions[i + 2]);
        }

        return _cachedVectors[index];
    }
}