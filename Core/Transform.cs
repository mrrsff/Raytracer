using System.Numerics;

namespace Raytracer.Core;

public class Transform
{
    public Vector3 Position
    {
        get => _position;
        set
        {
            translationMatrix = Matrix4x4.CreateTranslation(value);
            transformMatrix = scaleMatrix * rotationMatrix * translationMatrix;
            Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
            _position = value;
        }
    }

    public Quaternion Rotation 
    { 
        get => _rotation;
        set
        {
            rotationMatrix = Matrix4x4.CreateFromQuaternion(value);
            transformMatrix = scaleMatrix * rotationMatrix * translationMatrix;
            Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
            _rotation = value;
        }
    }
    public Vector3 Scale 
    { 
        get => _scale;
        set
        {
            scaleMatrix = Matrix4x4.CreateScale(value);
            transformMatrix = scaleMatrix * rotationMatrix * translationMatrix;
            Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
            _scale = value;
        }
    }
    
    private Vector3 _position;
    private Quaternion _rotation;
    private Vector3 _scale;
    
    private Matrix4x4 translationMatrix;
    private Matrix4x4 rotationMatrix;
    private Matrix4x4 scaleMatrix;
    
    private Matrix4x4 transformMatrix;
    private Matrix4x4 inverseTransformMatrix;
    
    public Transform Copy()
    {
        return new Transform
        {
            Position = Position,
            Rotation = Rotation,
            Scale = Scale
        };
    }
    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
    
    public Vector3 ToWorldPoint(Vector3 point)
    {
        return Vector3.Transform(point, transformMatrix);
    }
    public Vector3 ToLocalPoint(Vector3 point)
    {
        return Vector3.Transform(point, inverseTransformMatrix);
    }
    public Vector3 ToWorldDirection(Vector3 dir)
    {
        return Vector3.Normalize(Vector3.TransformNormal(dir, transformMatrix));
    }
    public Vector3 ToLocalDirection(Vector3 dir) 
    {
        return Vector3.Normalize(Vector3.TransformNormal(dir, inverseTransformMatrix));
    }
    public Ray ToLocalRay(Ray ray)
    {
        Vector3 localOrigin = ToLocalPoint(ray.Origin);
        Vector3 localDirection = ToLocalDirection(ray.Direction);
        return new Ray(localOrigin, localDirection, ray.IsSecondary);
    }
    public Ray ToWorldRay(Ray ray)
    {
        Vector3 worldOrigin = ToWorldPoint(ray.Origin);
        Vector3 worldDirection = ToWorldDirection(ray.Direction);
        return new Ray(worldOrigin, worldDirection, ray.IsSecondary);
    }
}