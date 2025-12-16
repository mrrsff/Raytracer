using System.Numerics;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public class MeshInstance
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_baseMeshId")] public int BaseMeshId;
    public int Material = -1;
    public string _resetTransform;
    public bool ResetTransform => _resetTransform == "true";
    public string Transformations;
    public Vector3 MotionBlur;
    public int[] Textures;

    public override string ToString()
    {
        return
            $"MeshInstance(Id: {Id}, BaseMeshId: {BaseMeshId}, ResetTransform: {ResetTransform}, TransformationData: {Transformations})";
    }
}