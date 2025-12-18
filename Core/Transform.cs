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

    public void Composite(Matrix4x4 matrix)
    {
        transformMatrix *= Matrix4x4.Transpose(matrix);
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void Translate(Vector3 translation)
    {
        Matrix4x4 translationMat = Matrix4x4.CreateTranslation(translation);
        transformMatrix *= translationMat;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void Rotate(Quaternion rotation)
    {
        Matrix4x4 rotationMat = Matrix4x4.CreateFromQuaternion(rotation);
        transformMatrix *= rotationMat;
        Matrix4x4.Invert(transformMatrix, out inverseTransformMatrix);
    }

    public void Rotate(Vector4 axisAngle)
    {
        Vector3 axis = new Vector3(axisAngle.X, axisAngle.Y, axisAngle.Z);
        float degrees = axisAngle.W;
        Rotate(axis, degrees);
    }

    public void Rotate(Vector3 axis, float degrees)
    {
        float radians = degrees * (float)(Math.PI / 180.0);
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), radians);
        Rotate(q);
    }

    public void Scale(Vector3 scale)
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
        return Vector3.Normalize(Vector3.TransformNormal(dir, transformMatrix));
    }
    
    public Vector3 ToWorldNormal(Vector3 normal)
    {
        Matrix4x4.Invert(transformMatrix, out Matrix4x4 inv);
        Matrix4x4 invTrans = Matrix4x4.Transpose(inv);
        return Vector3.Normalize(Vector3.TransformNormal(normal, invTrans));
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
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Transform Matrix:");
        sb.Append($"[{transformMatrix.M11:F2}, {transformMatrix.M21:F2}, {transformMatrix.M31:F2}, {transformMatrix.M41:F2}]\n");
        sb.Append($"[{transformMatrix.M12:F2}, {transformMatrix.M22:F2}, {transformMatrix.M32:F2}, {transformMatrix.M42:F2}]\n");
        sb.Append($"[{transformMatrix.M13:F2}, {transformMatrix.M23:F2}, {transformMatrix.M33:F2}, {transformMatrix.M43:F2}]\n");
        sb.Append($"[{transformMatrix.M14:F2}, {transformMatrix.M24:F2}, {transformMatrix.M34:F2}, {transformMatrix.M44:F2}]\n");
        return sb.ToString();
    }
    
    public static Transform Identity => new();
}