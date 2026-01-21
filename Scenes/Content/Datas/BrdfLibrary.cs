using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Rendering.Shading.BRDFs;

namespace Raytracer.Scenes.Content.Datas;

[Serializable]
public class BrdfLibrary
{
    [JsonConverter(typeof(SingleOrListConverter<BRDFDefinition>))]
    public List<BRDFDefinition> OriginalBlinnPhong;
    
    [JsonConverter(typeof(SingleOrListConverter<BRDFDefinition>))]
    public List<BRDFDefinition> ModifiedBlinnPhong;

    [JsonConverter(typeof(SingleOrListConverter<BRDFDefinition>))]
    public List<BRDFDefinition> OriginalPhong;

    [JsonConverter(typeof(SingleOrListConverter<BRDFDefinition>))]
    public List<BRDFDefinition> ModifiedPhong;
    
    [JsonConverter(typeof(SingleOrListConverter<BRDFDefinition>))]
    public List<BRDFDefinition> TorranceSparrow;
    
    private List<BRDFDefinition> allDefinitions;
    private void InitializeDefinitions()
    {
        if (allDefinitions != null)
            return;
        
        allDefinitions = new List<BRDFDefinition>();
        if (OriginalBlinnPhong != null)
        {
            foreach (var def in OriginalBlinnPhong)
            {
                def.Type = BRDFType.OriginalBlinnPhong;
                allDefinitions.Add(def);
            }
        }
        if (ModifiedBlinnPhong != null)
        {
            foreach (var def in ModifiedBlinnPhong)
            {
                def.Type = BRDFType.ModifiedBlinnPhong;
                allDefinitions.Add(def);
            }
        }
        if (OriginalPhong != null)
        {
            foreach (var def in OriginalPhong)
            {
                def.Type = BRDFType.OriginalPhong;
                allDefinitions.Add(def);
            }
        }
        if (ModifiedPhong != null)
        {
            foreach (var def in ModifiedPhong)
            {
                def.Type = BRDFType.ModifiedPhong;
                allDefinitions.Add(def);
            }
        }
        if (TorranceSparrow != null)
        {
            foreach (var def in TorranceSparrow)
            {
                def.Type = BRDFType.TorranceSparrow;
                allDefinitions.Add(def);
            }
        }
    }
    private BRDFDefinition? FindBRDFDefinitionById(int id)
    {
        return allDefinitions.FirstOrDefault(def => def.Id == id);
    }
    private IBRDF CreateBRDFFromDefinition(Material mat, BRDFDefinition def)
    {
        // Debug.Log($"Creating BRDF of type {def.Type} for Material {mat.Id} using BRDF Definition {def.Id}.");
        return def.Type switch
        {
            BRDFType.OriginalBlinnPhong => new OriginalBlinnPhongBRDF(def),
            BRDFType.ModifiedBlinnPhong => new ModifiedBlinnPhongBRDF(def),
            BRDFType.OriginalPhong => new OriginalPhongBRDF(def),
            BRDFType.ModifiedPhong => new ModifiedPhongBRDF(def),
            BRDFType.TorranceSparrow => new TorranceSparrowBRDF(mat, def),
            _ => throw new NotImplementedException($"BRDF Type {def.Type} is not implemented.")
        };
    }
    public IBRDF? CreateBRDF(Material mat)
    {
        BRDFDefinition? brdfDef = null;
        int? brdfId = mat.BrdfId;
        if (brdfId == null)
        {
            Debug.Log($"Material {mat.Id} does not have a valid BRDF ID.");
            return null;
        }
        
        InitializeDefinitions();
        brdfDef = FindBRDFDefinitionById(brdfId!.Value);
        return brdfDef == null ? 
            throw new KeyNotFoundException($"BRDF Definition with ID {brdfId} not found for Material {mat.Id}.") 
            : CreateBRDFFromDefinition(mat, brdfDef);
    }
}

public enum BRDFType
{
    OriginalBlinnPhong,
    ModifiedBlinnPhong,
    OriginalPhong,
    ModifiedPhong,
    TorranceSparrow
}
[Serializable]
public class BRDFDefinition
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_normalized")] public bool Normalized;
    [JsonConverter(typeof(StringToBoolConverter))]
    [JsonPropertyName("_kdfresnel")] public bool KDFresnel;
    
    public float Exponent;
    
    [JsonIgnore] public BRDFType Type; // Only internal
}