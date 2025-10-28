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
    
    
    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
    
    public Matrix4x4 GetTransformMatrix() => transformMatrix;
    public Matrix4x4 GetInverseTransformMatrix() => inverseTransformMatrix;
}