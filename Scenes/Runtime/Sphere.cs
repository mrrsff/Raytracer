using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Sphere
{
    public SphereData data;
    public float radiusSquared;
    
    public Sphere(SphereData data)
    {
        this.data = data;
        radiusSquared = data.Radius * data.Radius;
    }
}