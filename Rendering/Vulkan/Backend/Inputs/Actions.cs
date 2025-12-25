namespace Raytracer.Rendering.Vulkan.Backend.Inputs;

public enum InputAction
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    Sprint,
    Look,
    ToggleMouse
}
public enum ActionPhase
{
    Started,
    Performed,
    Canceled
}