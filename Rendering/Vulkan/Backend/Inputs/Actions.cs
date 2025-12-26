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
    ToggleMouse,
    IncreaseSpeed,
    DecreaseSpeed,
    IncreaseSensitivity,
    DecreaseSensitivity,
    EngageFlythrough, // Usually Right Mouse Button
    OrbitModifier,    // Usually Alt
    PanModifier       // Usually Middle Mouse Button
}
public enum ActionPhase
{
    Started,
    Performed,
    Canceled
}