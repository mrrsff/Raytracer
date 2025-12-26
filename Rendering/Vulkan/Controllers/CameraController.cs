using System;
using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Vulkan.Backend.Inputs;
using Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;
using Raytracer.Utility;

namespace Raytracer.Rendering.Vulkan.Controllers;

public sealed class CameraController
{
    private InputActionMap _actionMap;
    public Vector3 Position;
    
    private Vector3 _forward;
    private Vector3 _right;
    private Vector3 _up;
    
    public float Yaw;
    public float Pitch;

    public InputSettings Settings;
    public float MoveSpeed => Settings.MoveSpeed;
    public float LookSensitivity => Settings.LookSensitivity / 100f;

    public float VerticalFov = 60f;

    private Vector3 _moveIntent;
    private Vector2 _lookDelta;
    private bool isSprinting = false;
    public CameraController(InputActionMap input, InputSettings settings, CameraGpu? initial = null)
    {
        if (initial.HasValue)
        {
            Position = initial.Value.Position;
            _forward = initial.Value.Forward;
            ComputeBasisFromForward(_forward, out _right, out _up);
            Yaw = MathF.Atan2(_forward.Z, _forward.X);
            Pitch = MathF.Asin(_forward.Y);
            
            _cachedGpuCamera = initial.Value;
            _cachedDirty = false;
        }
        else
        {
            Position = Vector3.Zero;
            Yaw = 0f;
            Pitch = 0f;
            ComputeBasis(Yaw, Pitch, out _forward, out _right, out _up);
        }
        
        Settings = settings;
        _actionMap = input;
        _actionMap.ActionEvent += OnAction;
    }
    private bool mouseLocked = false;
    bool forward = false;
    bool backward = false;
    bool left = false;
    bool right = false;
    bool up = false;
    bool down = false;

    private void OnAction(InputActionEvent e)
    {
        if (e.Action == InputAction.MoveForward)
            forward = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.MoveBackward)
            backward = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.MoveRight)
            right = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.MoveLeft)
            left = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.MoveUp)
            up = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.MoveDown)
            down = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.Sprint)
            isSprinting = e.Phase != ActionPhase.Canceled;

        if (e.Action == InputAction.Look && e.Phase == ActionPhase.Performed)
            _lookDelta += e.Value;

        if (e.Action == InputAction.ToggleMouse && e.Phase == ActionPhase.Canceled)
        {
            _actionMap.ToggleMouseLock(!mouseLocked);
            mouseLocked = !mouseLocked;
        }

        _moveIntent = new Vector3(
            (right ? 1f : 0f) - (left ? 1f : 0f),
            (up ? 1f : 0f) - (down ? 1f : 0f),
            (forward ? 1f : 0f) - (backward ? 1f : 0f)
        );
    }

    private bool _dirty = true;
    public void Update(float deltaTime)
    {
        bool hasMovement = _moveIntent != Vector3.Zero;
        bool hasLook = _lookDelta != Vector2.Zero;
        if (!hasMovement && !hasLook)
            return;

        _dirty = true;
        
        if (hasLook)
        {
            Yaw += _lookDelta.X * LookSensitivity;
            Pitch -= _lookDelta.Y * LookSensitivity;
            
            const float MaxPitch = MathF.PI * 0.48f; // Just below 90 degrees to avoid gimbal lock
            Pitch = Math.Clamp(Pitch, -MaxPitch, MaxPitch);
            ComputeBasis(Yaw, Pitch, out _forward, out _right, out _up);
            // Debug.Log(_lookDelta);
            _lookDelta = Vector2.Zero;
        }
        if (hasMovement)
        {
            _moveIntent = Vector3.Normalize(_moveIntent);
            Vector3 move = 
                _forward * _moveIntent.Z +
                _right   * _moveIntent.X +
                _up      * _moveIntent.Y;
        
            if (move != Vector3.Zero)
                Position += Vector3.Normalize(move) * MoveSpeed * deltaTime * (isSprinting ? Settings.SprintMultiplier : 1f);
        }
    }
    
    private bool _cachedDirty = true;
    private CameraGpu _cachedGpuCamera;
    public CameraGpu BuildGpuCamera(float aspectRatio)
    {
        if (!_dirty && !_cachedDirty)
            return _cachedGpuCamera;
        if (_cachedDirty)
        {
            _cachedDirty = false;
            ComputeBasis(Yaw, Pitch, out _forward, out _right, out _up);
        }

        _dirty = false;
        _cachedGpuCamera = new CameraGpu
        {
            Position = Position,
            Forward = _forward,
            Right = _right,
            Up = _up,
            FovY = VerticalFov,
            AspectRatio = aspectRatio
        };
        return _cachedGpuCamera;
    }
    private static void ComputeBasis(float yaw, float pitch, out Vector3 forward, out Vector3 right, out Vector3 up)
    {
        forward = Vector3.Normalize(new Vector3(
            MathF.Cos(pitch) * MathF.Cos(yaw),
            MathF.Sin(pitch),
            MathF.Cos(pitch) * MathF.Sin(yaw)
        ));

        right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        up = Vector3.Normalize(Vector3.Cross(right, forward));
    }
    
    private static void ComputeBasisFromForward(Vector3 forward, out Vector3 right, out Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        up = Vector3.Normalize(Vector3.Cross(right, forward));
    }
}
