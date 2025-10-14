using System.Numerics;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Triangle
{
    public int I0, I1, I2;
    public Vector3 V0, V1, V2;
    
    public Vector3 E1; // V1 - V0
    public Vector3 E2; // V2 - V0

    public Vector3 Normal;

    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        E1 = v1 - v0;
        E2 = v2 - v0;
    }
    
    public Triangle(int i0, int i1, int i2, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        I0 = i0;
        I1 = i1;
        I2 = i2;
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        E1 = v1 - v0;
        E2 = v2 - v0;
    }
}