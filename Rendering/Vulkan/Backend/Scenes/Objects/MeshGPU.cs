using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct MeshGPU
{
    public int VertexOffset;
    public int VertexCount;
    
    public int TriangleOffset;
    public int TriangleCount;
    
    public int Flags; Vector3 _pad0;

    public override string ToString()
    {
        return $"MeshGPU(Vertices: {VertexCount} @ {VertexOffset}, Triangles: {TriangleCount} @ {TriangleOffset}, Flags: {Flags})";
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct MeshInstanceGPU
{
    public int MeshIndex;
    public int MaterialIndex; Vector2 _pad0;
    public MeshTransformGPU Transform;

    public override string ToString()
    {
        return $"MeshInstanceGPU(MeshIndex: {MeshIndex}, MaterialIndex: {MaterialIndex}, Transform: [{Transform.Row0}, {Transform.Row1}, {Transform.Row2}])";
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct MeshTransformGPU
{
    public Vector4 Row0;
    public Vector4 Row1;
    public Vector4 Row2;
    public Vector4 Row3; // Add the 4th row

    /// <summary>
    /// It assumes that the matrix is affine (no perspective), so the last row is (0,0,0,1)
    /// </summary>
    public static MeshTransformGPU Create(Matrix4x4 matrix)
    {
        return new MeshTransformGPU
        {
            Row0 = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14),
            Row1 = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24),
            Row2 = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34),
            Row3 = new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44)
        };
    }

    public override string ToString()
    {
        return $"[{Row0}, {Row1}, {Row2}, {Row3}]";
    }
}