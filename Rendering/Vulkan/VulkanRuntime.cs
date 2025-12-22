using Raytracer.Core;
using Raytracer.Rendering.Vulkan.Backend;
using Raytracer.Rendering.Vulkan.Backend.Allocations;
using Raytracer.Rendering.Vulkan.Backend.Inputs;
using Raytracer.Rendering.Vulkan.Backend.Scene;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Raytracer.Rendering.Vulkan;

public sealed class VulkanRuntime : IDisposable
{
    public InputHandler InputHandler => _inputHandler;
    public VKWindow Window => vkWindow;
    
    private VKWindow vkWindow;
    private VkContext _vkContext;
    private ComputePipeline _rayTracingPipeline;
    private VkImage _outputImage;
    private SwapchainPresenter _presenter;
    private InputHandler _inputHandler;
    private TimeManager _timeManager;
    private SceneResources _sceneResources;
    
    private uint width = 1280;
    private uint height = 720;
    private readonly string shaderPath;
    private readonly SceneDefinition _sceneDefinition;
    public VulkanRuntime(string shaderPath, SceneDefinition sceneDefinition)
    {
        this.shaderPath = shaderPath;
        _sceneDefinition = sceneDefinition;
        _timeManager = new TimeManager();
    }
    public void Start(Action onLoadCallback = null)
    {
        var options = WindowOptions.DefaultVulkan;
        options.Size = new Vector2D<int>((int)width, (int)height);
        options.Title = "Vulkan Raytracer";
        vkWindow = new VKWindow(options);
        vkWindow.OnLoad(OnLoad);
        if (onLoadCallback != null)
            vkWindow.OnLoad(onLoadCallback);
        vkWindow.OnResize(OnResize);
        vkWindow.OnRender(OnRender);
        vkWindow.OnUpdate(OnUpdate);
        vkWindow.Run();
    }

    private void OnUpdate(double obj)
    {
        _timeManager.Update(obj);
    }

    private volatile bool _resizePending;
    private void OnResize(Vector2D<int> obj)
    {
        width = (uint)obj.X;
        height = (uint)obj.Y;
        _resizePending = true;
    }

    private void OnRender(double dt)
    {
        if (width == 0 || height == 0)
            return;
        
        if (_resizePending)
        {
            HandleResize();
            _resizePending = false;
        }
        // _rayTracingPipeline.Dispatch(width, height);
        
        _presenter.Present(recorder => _rayTracingPipeline.Record(recorder, width, height), _outputImage);
    }
    private void HandleResize()
    {
        _vkContext.Vk.DeviceWaitIdle(_vkContext.Device);
        _outputImage.Dispose();
        _outputImage = _vkContext.CreateStorageImage(width, height);
        _rayTracingPipeline.UpdateOutputImage(_outputImage);
        _presenter.Resize();
    }

    public void Dispose()
    {
        _vkContext.Vk.DeviceWaitIdle(_vkContext.Device);
        _presenter.Dispose();
        _rayTracingPipeline.Dispose();
        _outputImage.Dispose();
        _vkContext.Dispose();
        vkWindow.Dispose();
    }
    
    private void OnLoad()
    {
        _vkContext = new VkContext(vkWindow._window);
        
        _sceneResources = new SceneResources(_sceneDefinition, _vkContext);
        
        _outputImage = _vkContext.CreateStorageImage((uint)vkWindow._window.Size.X, (uint)vkWindow._window.Size.Y);

        _rayTracingPipeline = new ComputePipeline(_vkContext, shaderPath, _outputImage, _sceneResources.DescriptorSet);
        
        _presenter = new SwapchainPresenter(_vkContext, vkWindow);
        
        _inputHandler = new InputHandler(vkWindow);
        
    }
}