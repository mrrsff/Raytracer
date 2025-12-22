using System.Numerics;

namespace Raytracer.Rendering.Vulkan.Backend.Inputs;

public readonly struct InputActionEvent
{
    public readonly InputAction Action;
    public readonly ActionPhase Phase;
    public readonly Vector2 Value; // zero for buttons, delta for axes

    public InputActionEvent(InputAction action, ActionPhase phase, Vector2 value)
    {
        Action = action;
        Phase = phase;
        Value = value;
    }
}