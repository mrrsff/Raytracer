using System.Numerics;
using Raytracer.Core;
using Silk.NET.Input;

namespace Raytracer.Rendering.Vulkan.Backend.Inputs;

public sealed class InputActionMap
{
    private readonly InputHandler _input;

    private readonly Dictionary<InputAction, List<InputBinding>> _bindings = new();

    public event Action<InputActionEvent>? ActionEvent;

    public InputActionMap(InputHandler input)
    {
        _input = input;

        _input.KeyDown += OnKeyDown;
        _input.KeyUp += OnKeyUp;
        _input.MouseDown += OnMouseDown;
        _input.MouseUp += OnMouseUp;
        _input.MouseMove += OnMouseMove;
        _input.MouseScroll += OnMouseScroll;
    }

    public void Bind(InputAction action, InputBinding binding)
    {
        if (!_bindings.TryGetValue(action, out var list))
        {
            list = [];
            _bindings[action] = list;
        }

        list.Add(binding);
    }
    private void Emit(InputAction action, ActionPhase phase, Vector2 value)
    {
        ActionEvent?.Invoke(new InputActionEvent(action, phase, value));
    }

    private IEnumerable<InputAction> Match(InputBinding binding)
    {
        foreach (var (action, list) in _bindings)
            if (list.Contains(binding))
                yield return action;
    }
    private void OnKeyDown(Key key)
    {
        foreach (var action in Match(new KeyBinding(key)))
        {
            Emit(action, ActionPhase.Started, Vector2.Zero);
        }
    }

    private void OnKeyUp(Key key)
    {
        foreach (var action in Match(new KeyBinding(key)))
            Emit(action, ActionPhase.Canceled, Vector2.Zero);
    }

    private void OnMouseDown(MouseButton button)
    {
        foreach (var action in Match(new MouseButtonBinding(button)))
            Emit(action, ActionPhase.Started, Vector2.Zero);
    }

    private void OnMouseUp(MouseButton button)
    {
        foreach (var action in Match(new MouseButtonBinding(button)))
            Emit(action, ActionPhase.Canceled, Vector2.Zero);
    }

    private void OnMouseMove(Vector2 delta)
    {
        foreach (var action in Match(new MouseAxisBinding(MouseAxis.X)))
            Emit(action, ActionPhase.Performed, new Vector2(delta.X, 0));

        foreach (var action in Match(new MouseAxisBinding(MouseAxis.Y)))
            Emit(action, ActionPhase.Performed, new Vector2(0, delta.Y));
        
        foreach (var action in Match(new MouseMoveBinding()))
            Emit(action, ActionPhase.Performed, delta);
    }

    private void OnMouseScroll(Vector2 scroll)
    {
        foreach (var action in Match(new MouseAxisBinding(MouseAxis.ScrollY)))
            Emit(action, ActionPhase.Performed, new Vector2(0, scroll.Y));
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("InputActionMap Bindings:");
        foreach (var (action, list) in _bindings)
        {
            sb.AppendLine($"  {action}:");
            foreach (var binding in list)
            {
                sb.AppendLine($"    - {binding}");
            }
        }

        return sb.ToString();
    }
    public void ToggleMouseLock(bool locked)
    {
        _input.SetMouseLock(locked);
    }
}