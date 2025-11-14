using System.Runtime.InteropServices;

namespace Raytracer.Scenes.Runtime.Meshes.BVH;

[StructLayout(LayoutKind.Sequential)]
public struct BVHNode
{
    public BoundingBox Bounds;
    public int LeftChild; // index in array, or -1 if leaf
    public int RightChild; // index in array, or -1 if leaf
    public int Start; // range start in triangle array
    public int End; // range end in triangle array
    public bool IsLeaf => LeftChild == -1;

    public override string ToString()
    {
        return IsLeaf
            ? $"Leaf: Tris[{Start}, {End}) BBox{Bounds}"
            : $"Node: Right={RightChild} BBox{Bounds}";
    }
}