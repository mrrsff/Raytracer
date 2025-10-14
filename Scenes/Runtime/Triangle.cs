using System.Numerics;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Triangle
{
    public Vector3 V0;
    public Vector3 V1;
    public Vector3 V2;

    public Vector3 Normal;

    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
    }
}