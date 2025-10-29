using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Scenes.Runtime;

public abstract class Geometry
{
    public int MaterialIndex { get; set; }
    public abstract bool Intersect(in Ray ray, ref IntersectionInfo info);
    public virtual int GetPrimitiveCount() => 1;
}