using System.Drawing.Printing;
using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Sampling;
using Raytracer.Rendering.Vulkan.Backend.Inputs;
using Raytracer.Rendering.Vulkan.Backend.Scenes;
using Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;
using Raytracer.Rendering.Vulkan.Controllers;
using Raytracer.Scenes;

namespace Raytracer.Rendering.Vulkan;

public class GPURendering : RenderingTechnique, IDisposable
{
    private VulkanRuntime _vulkanRuntime;
    private CameraController _cameraController;
    public static event Action<float> OnUpdate;
    public static event Action<float> OnRender;
    private float windowAspectRatio;
    private SceneDefinition sceneDefinition;
    public override void StartRendering(params string[] args)
    {
        var scenes = SceneProvider.GetScenes(Params.ScenePath);
        var firstScene = scenes.FirstOrDefault();
        if (firstScene == null) 
            throw new InvalidOperationException("No scenes found at the specified path.");
        firstScene.Initialize();
        sceneDefinition = new SceneDefinition(firstScene);
        
        var shaderPath = Path.Combine(Directory.GetCurrentDirectory(), "Shaders/raytracer.comp.spv");
        _vulkanRuntime = new VulkanRuntime(shaderPath);
        _vulkanRuntime.Start(OnLoad);
        _vulkanRuntime.Window.OnResize((size) =>
        {
            windowAspectRatio = (float)size.X / size.Y;
        });
    }

    private void OnLoad()
    {
        var size = _vulkanRuntime.Window._window.Size;
        windowAspectRatio = (float)size.X / size.Y;
        SetupInputHandling();
        _vulkanRuntime._sceneResources.SetFromScene(sceneDefinition);
        
        _vulkanRuntime.Window.OnUpdate((deltaTime) =>
        {
            _cameraController.Update((float)deltaTime);
            OnUpdate?.Invoke((float)deltaTime);
        });
        _vulkanRuntime.Window.OnRender((deltaTime) =>
        {
            UpdateCamera();
            OnRender?.Invoke((float)deltaTime);
        });
    }
    private void SetupInputHandling()
    {
        var userInput = ActionMaps.CreateUserActionMap(_vulkanRuntime.InputHandler);
        var settings = new InputSettings { MoveSpeed = 15f, LookSensitivity = 0.2f, SprintMultiplier = 4f};
        _cameraController = new CameraController(userInput, settings, sceneDefinition.Camera);
    }
    private void UpdateCamera()
    {
        CameraGpu cameraGpu = _cameraController.BuildGpuCamera(windowAspectRatio);
        _vulkanRuntime.UpdateCamera(cameraGpu);
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