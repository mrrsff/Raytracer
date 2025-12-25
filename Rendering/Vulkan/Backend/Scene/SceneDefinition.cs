namespace Raytracer.Rendering.Vulkan.Backend.Scene;

public class SceneDefinition
{
    private Scenes.Scene scene;
    
    public CameraGpu Camera;
    public SceneGlobals Globals;
    
    public SceneDefinition(Scenes.Scene scene)
    {
        this.scene = scene;
        ConvertToGpuData();
    }
    
    private void ConvertToGpuData()
    {
        Globals = new SceneGlobals
        {
            AmbientLightColor = scene.Content.BackgroundColor
        };

        var cam = scene.GetCamera(0);
        Camera = new CameraGpu
        {
            Position = cam.Position,
            Forward = cam.Forward,
            Right = cam.Right,
            Up = cam.Up,
            FovY = cam.FovY,
            AspectRatio = cam.ImageResolution.X / (float)cam.ImageResolution.Y
        };
    }
}