using System.Numerics;
using Raytracer.Core;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;

namespace Raytracer.Rendering.Vulkan.Backend.Inputs;

public sealed class InputHandler
{
    private readonly VKWindow _window;

    private IInputContext? _inputContext;

    private readonly HashSet<Key> _keysDown = new();
    private readonly HashSet<MouseButton> _mouseButtonsDown = new();

    private Vector2 _mousePosition;
    private Vector2 _mouseDelta;
    private Vector2 _scrollDelta;

    private bool _frozen;

    public InputHandler(VKWindow window)
    {
        _window = window;
        Initialize();
        _window.OnUpdate(_ => EndFrame());
    }
    private void Initialize()
    {
        _inputContext = _window._window.CreateInput();
        foreach (var keyboard in _inputContext.Keyboards)
        {
            SubscribeKeyboard(keyboard);
            break;
        }
        foreach (var mouse in _inputContext.Mice)
        {
            SubscribeMouse(mouse);
            break;
        }
    }
    public bool SetMouseLock(bool hidden)
    {
        var mouse = _inputContext?.Mice.FirstOrDefault();
        if (mouse == null) return false;

        mouse.Cursor.CursorMode = hidden ? CursorMode.Raw : CursorMode.Normal;
        return true;
    }

    private void SubscribeKeyboard(IKeyboard keyboard)
    {
        keyboard.KeyDown += OnKeyDownInternal;
        keyboard.KeyUp += OnKeyUpInternal;
    }
    
    private void SubscribeMouse(IMouse mouse)
    {
        mouse.MouseDown += OnMouseDownInternal;
        mouse.MouseUp += OnMouseUpInternal;
        mouse.MouseMove += OnMouseMoveInternal;
        mouse.Scroll += OnMouseScrollInternal;
    }
    
    public void Freeze()
    {
        _frozen = true;
        _keysDown.Clear();
        _mouseButtonsDown.Clear();
        _mouseDelta = Vector2.Zero;
        _scrollDelta = Vector2.Zero;
    }

    public void Unfreeze()
    {
        _frozen = false;
    }

    public bool IsFrozen => _frozen;

    private void OnKeyDownInternal(IKeyboard _, Key key, int __)
    {
        if (_frozen) return;

        if (_keysDown.Add(key))
        {
            KeyDown?.Invoke(key);
        }
    }

    private void OnKeyUpInternal(IKeyboard _, Key key, int __)
    {
        if (_frozen) return;

        if (_keysDown.Remove(key))
            KeyUp?.Invoke(key);
    }

    private void OnMouseDownInternal(IMouse _, MouseButton button)
    {
        if (_frozen) return;

        if (_mouseButtonsDown.Add(button))
            MouseDown?.Invoke(button);
    }

    private void OnMouseUpInternal(IMouse _, MouseButton button)
    {
        if (_frozen) return;

        if (_mouseButtonsDown.Remove(button))
            MouseUp?.Invoke(button);
    }

    private void OnMouseMoveInternal(IMouse _, Vector2 position)
    {
        if (_frozen) return;

        _mouseDelta += position - _mousePosition;
        _mousePosition = position;

        MouseMove?.Invoke(_mouseDelta);
    }

    private void OnMouseScrollInternal(IMouse _, ScrollWheel scroll)
    {
        if (_frozen) return;

        var delta = new Vector2(0, scroll.Y);
        _scrollDelta += delta;

        MouseScroll?.Invoke(delta);
    }

    private void EndFrame()
    {
        _mouseDelta = Vector2.Zero;
        _scrollDelta = Vector2.Zero;
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);
    public bool IsMouseDown(MouseButton button) => _mouseButtonsDown.Contains(button);

    public Vector2 MousePosition => _mousePosition;
    public Vector2 MouseDelta => _mouseDelta;
    public Vector2 ScrollDelta => _scrollDelta;

    public event Action<Key> KeyDown;
    public event Action<Key> KeyUp;
    public event Action<MouseButton> MouseDown;
    public event Action<MouseButton> MouseUp;
    public event Action<Vector2> MouseMove;
    public event Action<Vector2> MouseScroll;
}