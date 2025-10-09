using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.Core.Datas;
using Raytracer.IO.Converters;
using Raytracer.Scenes.Datas;

namespace Raytracer.Scenes;

public class SceneContent
{
    public Vector3 BackgroundColor;
    public float ShadowRayEpsilon;
    public float IntersectionTestEpsilon;
    public int MaxRecursionDepth;

    public Cameras Cameras;
    public Lights Lights;
    public Materials Materials;
    public VertexData VertexData;
    public SceneObjects SceneObjects;
}
public struct Cameras
{
    [JsonConverter(typeof(SingleOrArrayConverter<CameraData>))]
    public List<CameraData> CameraDatas;
}
public struct Lights
{
    public Vector3 AmbientLight;
    public List<PointLight> PointLights;
}

public struct Materials
{
    [JsonConverter(typeof(SingleOrArrayConverter<Material>))]
    public List<Material> MaterialDatas;
}

public struct SceneObjects
{
    public SphereData[] SphereDatas;
    public TriangleData[] TriangleDatas;
    public MeshData[] MeshDatas;
    public PlaneData[] PlaneDatas;
}