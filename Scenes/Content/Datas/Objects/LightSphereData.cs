using System.Numerics;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public class LightSphereData : SphereData
{
    public Vector3 Radiance;

    public override string ToString()
    {
        return $"LightSphere (ID: {Id}) - Center Vertex Index: {Center}, Radius: {Radius}, Material Index: {Material}, Radiance: {Radiance}";
    }
}