using Raytracer.Rendering.Vulkan.Backend.Inputs;
using Silk.NET.Input;

namespace Raytracer.Rendering.Vulkan;

public static class ActionMaps
{
    public static InputActionMap CreateUserActionMap(InputHandler input)
    {
        var map = new InputActionMap(input);
        
        map.Bind(InputAction.MoveForward, new KeyBinding(Key.W));
        map.Bind(InputAction.MoveBackward, new KeyBinding(Key.S));
        map.Bind(InputAction.MoveLeft, new KeyBinding(Key.A));
        map.Bind(InputAction.MoveRight, new KeyBinding(Key.D));
        map.Bind(InputAction.MoveUp, new KeyBinding(Key.Q));
        map.Bind(InputAction.MoveDown, new KeyBinding(Key.E));
        map.Bind(InputAction.Sprint, new KeyBinding(Key.ShiftLeft));
        map.Bind(InputAction.Look, new MouseMoveBinding());
        map.Bind(InputAction.ToggleMouse, new KeyBinding(Key.Escape));
        return map;
    }
    
}