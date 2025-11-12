using System.Numerics;

namespace Raytracer.Core;

public class Transform
{
    private Matrix4x4 transformMatrix;
    private Matrix4x4 inverseTransformMatrix;
    public Matrix4x4 Matrix => transformMatrix;
    public Matrix4x4 InverseMatrix => inverseTransformMatrix;

    public Transform()
    {
        transformMatrix = Matrix4x4.Identity;
        inverseTransformMatrix = Matrix4x4.Identity;
    }

    public Transform(Matrix4x4 matrix)
    {
        transformMatrix = matrix;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void SetMatrix(Matrix4x4 matrix)
    {
        transformMatrix = matrix;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void ApplyTranslation(Vector3 translation)
    {
        Matrix4x4 translationMat = Matrix4x4.CreateTranslation(translation);
        transformMatrix *= translationMat;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void ApplyRotation(Quaternion rotation)
    {
        Matrix4x4 rotationMat = Matrix4x4.CreateFromQuaternion(rotation);
        transformMatrix *= rotationMat;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void ApplyRotation(Vector4 axisAngle)
    {
        Vector3 axis = new Vector3(axisAngle.X, axisAngle.Y, axisAngle.Z);
        float degrees = axisAngle.W;
        ApplyRotation(axis, degrees);
    }

    public void ApplyRotation(Vector3 axis, float degrees)
    {
        float radians = degrees * (float)(Math.PI / 180.0);
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), radians);
        ApplyRotation(q);
    }

    public void ApplyScale(Vector3 scale)
    {
        Matrix4x4 scaleMat = Matrix4x4.CreateScale(scale);
        transformMatrix *= scaleMat;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
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
        Matrix4x4 invT = Matrix4x4.Transpose(inverseTransformMatrix);
        return Vector3.Normalize(Vector3.TransformNormal(dir, invT));
    }

    public Vector3 ToLocalDirection(Vector3 dir)
    {
        return Vector3.Normalize(Vector3.TransformNormal(dir, inverseTransformMatrix));
    }

    public Ray ToLocalRay(Ray ray)
    {
        Vector3 o = ToLocalPoint(ray.Origin);
        Vector3 d = ToLocalDirection(ray.Direction);
        return new Ray(o, d, ray.IsSecondary);
    }

    public Ray ToWorldRay(Ray ray)
    {
        Vector3 o = ToWorldPoint(ray.Origin);
        Vector3 d = ToWorldDirection(ray.Direction);
        return new Ray(o, d, ray.IsSecondary);
    }

    public Transform Copy()
    {
        return new Transform(transformMatrix);
    }

    public override string ToString()
    {
        return transformMatrix.ToString();
    }
}