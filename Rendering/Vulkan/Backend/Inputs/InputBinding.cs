using Silk.NET.Input;

namespace Raytracer.Rendering.Vulkan.Backend.Inputs;

public abstract record InputBinding;

public record KeyBinding(Key Key) : InputBinding;
public record MouseButtonBinding(MouseButton Button) : InputBinding;
public record MouseAxisBinding(MouseAxis Axis) : InputBinding;
public record MouseMoveBinding() : InputBinding;
public enum MouseAxis
{
    X,
    Y,
    ScrollY
}