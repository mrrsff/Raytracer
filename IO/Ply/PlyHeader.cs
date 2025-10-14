namespace Raytracer.IO.Ply;

public class PlyHeader
{
    public PlyImporter.PlyFormat Format = PlyImporter.PlyFormat.Ascii;
    public int VertexCount;
    public int FaceCount;
    public readonly List<(string type, string name)> VertexProps = new();
    public string FaceCountType = "uchar";
    public string FaceIndexType = "int";
    public long HeaderEnd;
}