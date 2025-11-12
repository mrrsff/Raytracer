using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.SceneLoaders.Converters;

namespace Raytracer.Scenes.Content.Datas;

public struct Transformations
{
    [JsonConverter(typeof(SingleOrListConverter<TransformationEntry>))]
    public List<TransformationEntry> Scaling;
    
    [JsonConverter(typeof(SingleOrListConverter<TransformationEntry>))]
    public List<TransformationEntry> Rotation;
    
    [JsonConverter(typeof(SingleOrListConverter<TransformationEntry>))]
    public List<TransformationEntry> Translation;
    
    public override string ToString()
    {
        // Check for null lists to avoid null reference exceptions
        Scaling ??= [];
        Rotation ??= [];
        Translation ??= [];
        StringBuilder sb = new();
        sb.AppendLine("Transformations:");
        if (Translation.Count > 0)
        {
            sb.AppendLine("  Translation:");
            foreach (var translation in Translation)
            {
                sb.AppendLine($"    {translation}");
            }
        }
        if (Rotation.Count > 0)
        {
            sb.AppendLine("  Rotation:");
            foreach (var rotation in Rotation)
            {
                sb.AppendLine($"    {rotation}");
            }
        }
        if (Scaling.Count > 0)
        {
            sb.AppendLine("  Scaling:");
            foreach (var scaling in Scaling)
            {
                sb.AppendLine($"    {scaling}");
            }
        }
        return sb.ToString();
    }

    public void ApplyTransformations(Transform transform, string transformOrder)
    {
        var ids = transformOrder.Split(' ');

        for (int i = 0; i < ids.Length; i++)
        {
            string idStr = ids[i];

            if (idStr.StartsWith("t"))
            {
                var entry = Translation.FirstOrDefault(e => e.Id.ToString() == idStr[1..]);
                if (entry.Data != null)
                    transform.ApplyTranslation(entry.ToVector3());
            }
            else if (idStr.StartsWith("r"))
            {
                var entry = Rotation.FirstOrDefault(e => e.Id.ToString() == idStr[1..]);
                if (entry.Data != null)
                    transform.ApplyRotation(entry.ToRotation());
            }
            else if (idStr.StartsWith("s"))
            {
                var entry = Scaling.FirstOrDefault(e => e.Id.ToString() == idStr[1..]);
                if (entry.Data != null)
                    transform.ApplyScale(entry.ToVector3());
            }
            else if (idStr.StartsWith("c"))
            {
                var entry = Translation.FirstOrDefault(e => e.Id.ToString() == idStr[1..]);
                if (entry.Data != null)
                    transform.SetMatrix(entry.ToCompositeMatrix());
            }
        }
    }
}
public struct TransformationEntry
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_data")] public float[] Data;

    public override string ToString()
    {
        return $"TransformationEntry(Id: {Id}, Data: [{string.Join(", ", Data)}])";
    }
    
    public Vector3 ToVector3()
    {
        if (Data.Length != 3)
            throw new InvalidOperationException("Data length is not 3 for Vector3 conversion.");
        return new Vector3(Data[0], Data[1], Data[2]);
    }
    
    public Vector4 ToRotation()
    {
        if (Data.Length != 4)
            throw new InvalidOperationException("Data length is not 4 for Rotation conversion.");
        return new Vector4(Data[1], Data[2], Data[3], Data[0]);
    }
    public Matrix4x4 ToCompositeMatrix()
    {
        if (Data.Length != 16)
            throw new InvalidOperationException("Data length is not 16 for Matrix4x4 conversion.");
        return new Matrix4x4(
            Data[0], Data[1], Data[2], Data[3],
            Data[4], Data[5], Data[6], Data[7],
            Data[8], Data[9], Data[10], Data[11],
            Data[12], Data[13], Data[14], Data[15]
        );
    }
}