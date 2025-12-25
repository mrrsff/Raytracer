using System.Drawing.Printing;
using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Sampling;
using Raytracer.Rendering.Vulkan.Backend.Inputs;
using Raytracer.Rendering.Vulkan.Backend.Scene;
using Raytracer.Rendering.Vulkan.Backend.Scene.Objects;
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
    public override void StartRendering(params string[] args)
    {
        var scenes = SceneProvider.GetScenes(Params.ScenePath);
        SceneDefinition sceneDefinition = new SceneDefinition(scenes.First());
        
        var shaderPath = Path.Combine(Directory.GetCurrentDirectory(), "Shaders/raytracer.comp.spv");
        _vulkanRuntime = new VulkanRuntime(shaderPath, sceneDefinition);
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
        SetSpheres();
        SetLights();
        SetSceneGlobals();
        
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
        _cameraController = new CameraController(userInput, settings);
    }
    private void UpdateCamera()
    {
        CameraGpu cameraGpu = _cameraController.BuildGpuCamera(windowAspectRatio);
        _vulkanRuntime.UpdateCamera(cameraGpu);
    }
    
    private void SetSpheres()
    {
        var spheres = new SphereGPU[10];
        spheres[0] = new SphereGPU
        {
            Center = new Vector3(0f, 0f, 10f),
            Radius = 0.3f,
        };
        
        
        for (int i = 1; i < spheres.Length; i++)
        {
            spheres[i] = new SphereGPU
            {
                Center = new Vector3(ThreadRng.NextFloat(-10f, 10f), ThreadRng.NextFloat(-10f, 10f),
                    ThreadRng.NextFloat(-10f, 10f)),
                Radius = ThreadRng.NextFloat(0.5f, 2f),
            };
        }
        _vulkanRuntime.UpdateSpheres(spheres);
    }
    
    private void SetLights()
    {
        var lights = new PointLightGPU[20];
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i] = new PointLightGPU
            {
                position = new Vector3(ThreadRng.NextFloat(-15f, 15f), ThreadRng.NextFloat(-15f, 15f),
                    ThreadRng.NextFloat(-15f, 15f)),
                color = new Vector3(ThreadRng.NextFloat(0.5f, 1f), ThreadRng.NextFloat(0.5f, 1f),
                    ThreadRng.NextFloat(0.5f, 1f)),
                intensity = ThreadRng.NextFloat(5f, 20f)
            };
        }
        _vulkanRuntime.UpdateLights(lights);
    }
    private void SetSceneGlobals()
    {
        SceneGlobals globals = new SceneGlobals
        {
            AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f),
            NumPointLights = 20,
            NumSpheres = 10
        };
        _vulkanRuntime.UpdateSceneGlobals(globals);
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