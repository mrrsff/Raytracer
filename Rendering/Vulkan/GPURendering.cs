namespace Raytracer.Rendering.Vulkan;

public class GPURendering : RenderingTechnique, IDisposable
{
    private VulkanRuntime _vulkanRuntime;
    public override void StartRendering(params string[] args)
    {
        var shaderPath = Path.Combine(Directory.GetCurrentDirectory(), "Shaders/raytracer.comp.spv");
        _vulkanRuntime = new VulkanRuntime(shaderPath);
        _vulkanRuntime.Start(SetupInputHandling);
    }
    
    private void SetupInputHandling()
    {
        var userInput = ActionMaps.CreateUserActionMap(_vulkanRuntime.InputHandler);
    }
    
    ~GPURendering()
    {
        Dispose();
    }

    public void Dispose()
    {
        _vulkanRuntime.Dispose();
        GC.SuppressFinalize(this);
    }
}