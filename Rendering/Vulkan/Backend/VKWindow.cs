using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Raytracer.Rendering.Vulkan.Backend;

public class VKWindow : IDisposable
{
    public IWindow _window { get; init; }
    public VKWindow(WindowOptions options)
    {
        _window = Window.Create(options);
    }
    
    public void Run() => _window.Run();

    public void OnLoad(Action action) => _window.Load += action;
    public void OnClose(Action action) => _window.Closing += action;
    public void OnUpdate(Action<double> action) => _window.Update += action;
    public void OnRender(Action<double> action) => _window.Render += action;
    public void OnResize(Action<Vector2D<int>> action) => _window.Resize += action;
    public void OnFileDrop(Action<string[]> action) => _window.FileDrop += action;
    public void OnMove(Action<Vector2D<int>> action) => _window.Move += action;
    public void OnStateChanged(Action<WindowState> action) => _window.StateChanged += action;
    public void OnFocusChanged(Action<bool> action) => _window.FocusChanged += action;
    public void OnFramebufferResize(Action<Vector2D<int>> action) => _window.FramebufferResize += action;


    public void Dispose()
    {
        _window.Dispose();
    }
}
