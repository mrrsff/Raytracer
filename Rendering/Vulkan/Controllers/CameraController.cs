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
            VerticalFov = initial.Value.FovY;
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
    
    private bool _isRMBPressed = false;
    private bool _isAltPressed = false;
    private bool _isMiddleMousePressed = false;

    private void OnAction(InputActionEvent e)
    {
        // 1. Capture Engagement States
        if (e.Action == InputAction.EngageFlythrough)
        {
            _isRMBPressed = e.Phase != ActionPhase.Canceled;
            _actionMap.ToggleMouseLock(_isRMBPressed); // Auto-lock mouse when looking
        }
        if (e.Action == InputAction.OrbitModifier)
            _isAltPressed = e.Phase != ActionPhase.Canceled;
        
        if (e.Action == InputAction.PanModifier)
            _isMiddleMousePressed = e.Phase != ActionPhase.Canceled;

        // 2. Only allow WASD movement if RMB is held (Unity Flythrough)
        if (_isRMBPressed)
        {
            if (e.Action == InputAction.MoveForward) forward = e.Phase != ActionPhase.Canceled;
            if (e.Action == InputAction.MoveBackward) backward = e.Phase != ActionPhase.Canceled;
            if (e.Action == InputAction.MoveRight) right = e.Phase != ActionPhase.Canceled;
            if (e.Action == InputAction.MoveLeft) left = e.Phase != ActionPhase.Canceled;
            if (e.Action == InputAction.MoveUp) up = e.Phase != ActionPhase.Canceled;
            if (e.Action == InputAction.MoveDown) down = e.Phase != ActionPhase.Canceled;
            if (e.Action == InputAction.Sprint) isSprinting = e.Phase != ActionPhase.Canceled;
        }
        else
        {
            // Reset movement if RMB is released
            forward = backward = left = right = up = down = false;
        }

        // 3. Capture Look Delta
        if (e.Action == InputAction.Look && e.Phase == ActionPhase.Performed)
        {
            _lookDelta += e.Value;
        }
        
        if (e.Action == InputAction.IncreaseSpeed && e.Phase == ActionPhase.Canceled)
        {
            SetMoveSpeed(Settings.MoveSpeed * 1.1f);
        }
        if (e.Action == InputAction.DecreaseSpeed && e.Phase == ActionPhase.Canceled)
        {
            SetMoveSpeed(Settings.MoveSpeed / 1.1f);
        }
        if (e.Action == InputAction.IncreaseSensitivity && e.Phase == ActionPhase.Canceled)
        {
            SetLookSensitivity(Settings.LookSensitivity * 1.1f);
        }
        if (e.Action == InputAction.DecreaseSensitivity && e.Phase == ActionPhase.Canceled)
        {
            SetLookSensitivity(Settings.LookSensitivity / 1.1f);
        }

        _moveIntent = new Vector3(
            (right ? 1f : 0f) - (left ? 1f : 0f),
            (up ? 1f : 0f) - (down ? 1f : 0f),
            (forward ? 1f : 0f) - (backward ? 1f : 0f)
        );
    }
    
    private void SetMoveSpeed(float newSpeed)
    {
        if (MathF.Abs(Settings.MoveSpeed - newSpeed) < 1e-6f)
            return;

        float old = Settings.MoveSpeed;
        Settings.MoveSpeed = newSpeed;
        Debug.Log($"[Camera] MoveSpeed changed: {old} → {newSpeed}");
    }

    private void SetLookSensitivity(float newSensitivity)
    {
        if (MathF.Abs(Settings.LookSensitivity - newSensitivity) < 1e-6f)
            return;

        float old = Settings.LookSensitivity;
        Settings.LookSensitivity = newSensitivity;
        Debug.Log($"[Camera] LookSensitivity changed: {old} → {newSensitivity}");
    }

    private bool _dirty = true;
    public void Update(float deltaTime)
    {
        bool hasLook = _lookDelta != Vector2.Zero;
        bool hasMovement = _moveIntent != Vector3.Zero;

        if (_isRMBPressed && hasLook)
        {
            // Unity-style Flythrough Rotation
            Yaw += _lookDelta.X * LookSensitivity;
            Pitch -= _lookDelta.Y * LookSensitivity;
            Pitch = Math.Clamp(Pitch, -MathF.PI * 0.48f, MathF.PI * 0.48f);
            ComputeBasis(Yaw, Pitch, out _forward, out _right, out _up);
            _dirty = true;
        }
        else if (_isMiddleMousePressed && hasLook)
        {
            // Unity-style Panning (Middle Mouse)
            float panSpeed = MoveSpeed * 0.05f; 
            Position -= _right * _lookDelta.X * panSpeed * deltaTime;
            Position += _up * _lookDelta.Y * panSpeed * deltaTime;
            _dirty = true;
        }

        if (hasMovement)
        {
            // Standard WASD Flythrough
            _moveIntent = Vector3.Normalize(_moveIntent);
            Vector3 move = (_forward * _moveIntent.Z) + (_right * _moveIntent.X) + (_up * _moveIntent.Y);
            Position += move * MoveSpeed * deltaTime * (isSprinting ? Settings.SprintMultiplier : 1f);
            _dirty = true;
        }

        _lookDelta = Vector2.Zero;
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
