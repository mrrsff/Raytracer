using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

namespace Raytracer.IO.Ply
{
    public static class PlyImporter
    {
        public enum PlyFormat { Ascii, BinaryLittleEndian, BinaryBigEndian }

        private sealed class PlyHeader
        {
            public PlyFormat Format = PlyFormat.Ascii;
            public int VertexCount;
            public int FaceCount;
            public readonly List<(string type, string name)> VertexProps = new();
            public string FaceCountType = "uchar"; // list count type
            public string FaceIndexType = "int";   // list index type
            public long HeaderEnd;                 // byte offset immediately after end_header line
        }

        public static (Vector3[] vertices, int[][] faces) Parse(string path)
        {
            path = Path.Combine(Program.WorkingDirectory, path);
            var header = ReadHeaderRaw(path);
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.Position = header.HeaderEnd;

            return header.Format == PlyFormat.Ascii
                ? ParseAscii(fs, header)
                : ParseBinary(fs, header);
        }

        private static PlyHeader ReadHeaderRaw(string path)
        {
            var h = new PlyHeader();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            bool inVertex = false, inFace = false;
            while (true)
            {
                string? line = ReadAsciiLine(fs);
                if (line == null) throw new Exception("Unexpected EOF before end_header.");

                var s = line.TrimEnd('\r');
                if (s.Length == 0) continue;

                if (s == "ply") continue;

                if (s.StartsWith("format "))
                {
                    if (s.Contains("ascii")) h.Format = PlyFormat.Ascii;
                    else if (s.Contains("binary_little_endian")) h.Format = PlyFormat.BinaryLittleEndian;
                    else if (s.Contains("binary_big_endian"))    h.Format = PlyFormat.BinaryBigEndian;
                    else throw new Exception($"Unsupported PLY format: {s}");
                }
                else if (s.StartsWith("element vertex"))
                {
                    h.VertexCount = int.Parse(s.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2], CultureInfo.InvariantCulture);
                    inVertex = true; inFace = false;
                }
                else if (s.StartsWith("element face"))
                {
                    h.FaceCount = int.Parse(s.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2], CultureInfo.InvariantCulture);
                    inVertex = false; inFace = true;
                }
                else if (s.StartsWith("property list") && inFace)
                {
                    // e.g. "property list uchar int vertex_indices"
                    var p = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    // p[0]=property, p[1]=list, p[2]=countType, p[3]=indexType, p[4]=name
                    h.FaceCountType = p[2];
                    h.FaceIndexType = p[3];
                }
                else if (s.StartsWith("property ") && inVertex)
                {
                    var p = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    h.VertexProps.Add((p[1], p[2]));
                }
                else if (s.StartsWith("end_header"))
                {
                    h.HeaderEnd = fs.Position;
                    break;
                }
            }

            if (h.VertexCount <= 0) throw new Exception("PLY: vertex count missing/invalid.");
            return h;
        }

        private static string? ReadAsciiLine(FileStream fs)
        {
            var bytes = new List<byte>(128);
            int b;
            while ((b = fs.ReadByte()) != -1)
            {
                if (b == '\n') break;
                bytes.Add((byte)b);
            }
            if (b == -1 && bytes.Count == 0) return null;
            return Encoding.ASCII.GetString(bytes.ToArray());
        }
        private static (Vector3[] vertices, int[][] faces) ParseAscii(Stream s, PlyHeader h)
        {
            using var r = new StreamReader(s, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var verts = new Vector3[h.VertexCount];
            var faces = new int[h.FaceCount][];
            
            for (int i = 0; i < h.VertexCount; i++)
            {
                var line = r.ReadLine() ?? throw new Exception("Unexpected EOF in ASCII vertex list.");
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Map by property names
                float x = 0, y = 0, z = 0;
                for (int p = 0, f = 0; p < h.VertexProps.Count && p < parts.Length; p++)
                {
                    if (h.VertexProps[p].type.StartsWith("float"))
                    {
                        float val = float.Parse(parts[p], CultureInfo.InvariantCulture);
                        var name = h.VertexProps[p].name;
                        if (name == "x") x = val; else if (name == "y") y = val; else if (name == "z") z = val;
                        f++;
                    }
                }
                verts[i] = new Vector3(x, y, z);
            }
            for (int i = 0; i < h.FaceCount; i++)
            {
                var line = r.ReadLine() ?? throw new Exception("Unexpected EOF in ASCII face list.");
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int n = int.Parse(parts[0], CultureInfo.InvariantCulture);
                var idx = new int[n];
                for (int j = 0; j < n; j++) idx[j] = int.Parse(parts[j + 1], CultureInfo.InvariantCulture);
                faces[i] = idx;
            }
            return (verts, faces);
        }
        private static (Vector3[] vertices, int[][] faces) ParseBinary(Stream s, PlyHeader h)
        {
            using var br = new BinaryReader(s, Encoding.ASCII, leaveOpen: true);

            int propCount = h.VertexProps.Count;
            int[] propSizes = new int[propCount];
            for (int i = 0; i < propCount; i++) propSizes[i] = SizeOf(h.VertexProps[i].type);

            var vertices = new Vector3[h.VertexCount];

            for (int i = 0; i < h.VertexCount; i++)
            {
                float x = 0, y = 0, z = 0, nx = 0, ny = 0, nz = 0;
                for (int p = 0; p < propCount; p++)
                {
                    var (type, name) = h.VertexProps[p];
                    switch (type)
                    {
                        case "float":
                        case "float32":
                            float fv = ReadFloat(br, h.Format);
                            switch (name)
                            {
                                case "x":
                                    x = fv;
                                    break;
                                case "y":
                                    y = fv;
                                    break;
                                case "z":
                                    z = fv;
                                    break;
                                case "nx":
                                    nx = fv;
                                    break;
                                case "ny":
                                    ny = fv;
                                    break;
                                case "nz":
                                    nz = fv;
                                    break;
                            }
                            break;
                        case "double":
                        case "float64":
                            _ = ReadDouble(br, h.Format); // skip or store if needed
                            break;
                        case "char":
                        case "int8":
                        case "uchar":
                        case "uint8":
                            br.ReadByte(); // skip
                            break;
                        case "short":
                        case "int16":
                        case "ushort":
                        case "uint16":
                            _ = ReadUInt16(br, h.Format); // skip
                            break;
                        case "int":
                        case "int32":
                        case "uint":
                        case "uint32":
                            _ = ReadUInt32(br, h.Format); // skip
                            break;
                        default:
                            br.ReadBytes(propSizes[p]);
                            break;
                    }
                }
                vertices[i] = new Vector3(x, y, z);
            }

            var faces = new int[h.FaceCount][];
            for (int i = 0; i < h.FaceCount; i++)
            {
                int n = ReadCount(br, h.FaceCountType, h.Format);
                var idx = new int[n];
                for (int j = 0; j < n; j++) idx[j] = ReadIndex(br, h.FaceIndexType, h.Format);
                faces[i] = idx;
            }

            return (vertices, faces);
        }
        private static int SizeOf(string t) => t switch
        {
            "char" or "uchar" or "int8" or "uint8" => 1,
            "short" or "ushort" or "int16" or "uint16" => 2,
            "int" or "uint" or "float" or "int32" or "uint32" or "float32" => 4,
            "double" or "float64" => 8,
            _ => 4
        };

        private static float ReadFloat(BinaryReader br, PlyFormat fmt)
        {
            var b = br.ReadBytes(4);
            if (fmt == PlyFormat.BinaryBigEndian) Array.Reverse(b);
            return BitConverter.ToSingle(b, 0);
        }

        private static double ReadDouble(BinaryReader br, PlyFormat fmt)
        {
            var b = br.ReadBytes(8);
            if (fmt == PlyFormat.BinaryBigEndian) Array.Reverse(b);
            return BitConverter.ToDouble(b, 0);
        }

        private static ushort ReadUInt16(BinaryReader br, PlyFormat fmt)
        {
            var b = br.ReadBytes(2);
            if (fmt == PlyFormat.BinaryBigEndian) Array.Reverse(b);
            return BitConverter.ToUInt16(b, 0);
        }

        private static uint ReadUInt32(BinaryReader br, PlyFormat fmt)
        {
            var b = br.ReadBytes(4);
            if (fmt == PlyFormat.BinaryBigEndian) Array.Reverse(b);
            return BitConverter.ToUInt32(b, 0);
        }

        private static int ReadCount(BinaryReader br, string type, PlyFormat fmt) => type switch
        {
            "uchar" or "uint8" => br.ReadByte(),
            "char" or "int8" => (sbyte)br.ReadByte(),
            "ushort" or "uint16" => ReadUInt16(br, fmt),
            "short" or "int16" => (short)ReadUInt16(br, fmt),
            "uint" or "uint32" => (int)ReadUInt32(br, fmt),
            "int" or "int32" => (int)ReadUInt32(br, fmt), // PLY often uses signed, but casting to int is fine
            _ => br.ReadByte()
        };

        private static int ReadIndex(BinaryReader br, string type, PlyFormat fmt) => type switch
        {
            "uint" or "uint32" => unchecked((int)ReadUInt32(br, fmt)),
            "int" or "int32" => unchecked((int)ReadUInt32(br, fmt)),
            "ushort" or "uint16" => ReadUInt16(br, fmt),
            "short" or "int16" => (short)ReadUInt16(br, fmt),
            "uchar" or "uint8" => br.ReadByte(),
            "char" or "int8" => (sbyte)br.ReadByte(),
            _ => unchecked((int)ReadUInt32(br, fmt))
        };
    }
}
