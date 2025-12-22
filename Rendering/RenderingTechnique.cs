namespace Raytracer.Rendering;

public abstract class RenderingTechnique
{
    public abstract void StartRendering(params string[] args);
}