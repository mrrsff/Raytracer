namespace Raytracer.Core.Datas;

public enum FaceType
{
    triangle,
    quad
}
public struct FacesData
{
    public List<int> Data;
    public FaceType Type;
}