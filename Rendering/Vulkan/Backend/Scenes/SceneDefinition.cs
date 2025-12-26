using System.Globalization;
using System.Numerics;
using System.Text;
using Raytracer.Core;
using Raytracer.IO.Meshes;
using Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;
using Raytracer.Scenes;
using Raytracer.Scenes.Runtime.Meshes;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes;

public class SceneDefinition
{
    private Scene scene;
    
    public CameraGpu Camera;
    public SceneGlobals Globals;
    
    public PointLightGPU[] PointLights;
    
    public MeshGPU[] Meshes;
    public MeshInstanceGPU[] MeshInstances;
    public TriangleGPU[] Triangles;
    public VertexGPU[] Vertices;
    
    public MaterialGPU[] Materials;
    
    public SceneDefinition(Scene scene)
    {
        this.scene = scene;
        Globals = new SceneGlobals
        {
            AmbientLightColor = scene.Content.BackgroundColor
        };
        
        CreateCamera();
        CreateLights();
        CreateObjects();
        CreateMaterials();
    }

    private void CreateCamera()
    {
        var cam = scene.GetCamera(0);
        Camera = new CameraGpu
        {
            Position = cam.Position,
            Forward = cam.Forward,
            Right = cam.Right,
            Up = cam.Up,
            FovY = cam.FovY > 0 ? cam.FovY : 60.0f,
            AspectRatio = cam.ImageResolution.X / (float)cam.ImageResolution.Y
        };
    }

    private void CreateLights()
    {
        var pointLights = scene.Content.Lights.PointLight;
        PointLights = new PointLightGPU[pointLights.Count];
        for (int i = 0; i < pointLights.Count; i++)
        {
            var light = pointLights[i];
            PointLights[i] = new PointLightGPU
            {
                position = light.Position,
                intensity = light.Intensity
            };
        }

        Globals.NumPointLights = PointLights.Length;
    }

    private void CreateObjects()
    {
        var meshDict = new Dictionary<int, int>(); // to link mesh definitions to indices
        Meshes = new MeshGPU[scene.MeshDefinitions.Count];
        Vertices = new VertexGPU[scene.MeshDefinitions.Sum(m => m.Vertices.Length)];
        Triangles = new TriangleGPU[scene.MeshDefinitions.Sum(m => m.Triangles.Length)];

        int currentVertexOffset = 0;
        int currentTriangleOffset = 0;
        for (int i = 0; i < scene.MeshDefinitions.Count; i++)
        {
            // Create mesh GPU representation
            var meshDefinition = scene.MeshDefinitions[i];
            meshDict.Add(meshDefinition.Id, i);

            Meshes[i] = new MeshGPU
            {
                VertexOffset = currentVertexOffset,
                VertexCount = meshDefinition.Vertices.Length,
                TriangleOffset = currentTriangleOffset,
                TriangleCount = meshDefinition.Triangles.Length
            };

            // Create vertices
            for (int v = 0; v < meshDefinition.Vertices.Length; v++)
            {
                Vertices[currentVertexOffset + v] = new VertexGPU
                {
                    Position = meshDefinition.Vertices[v],
                    Normal = meshDefinition.VertexNormals[v],
                    UV = meshDefinition.TexCoords != null && meshDefinition.TexCoords.Length > v
                        ? meshDefinition.TexCoords[v]
                        : new Vector2(0, 0)
                };
            }

            // Create triangles
            for (int t = 0; t < meshDefinition.Triangles.Length; t++)
            {
                var tri = meshDefinition.Triangles[t];
                Triangles[currentTriangleOffset + t] = new TriangleGPU
                {
                    Vertex0 = tri.I0,
                    Vertex1 = tri.I1,
                    Vertex2 = tri.I2,
                };
            }
            
            currentVertexOffset += meshDefinition.Vertices.Length;
            currentTriangleOffset += meshDefinition.Triangles.Length;
            
            // MeshExporter.ExportToOBJ($"MeshGPU_{i}_Export.obj", Meshes[i], Vertices, Triangles);
        }

        var runtimeMeshes = scene.Geometries.OfType<Mesh>().ToArray();
        MeshInstances = new MeshInstanceGPU[runtimeMeshes.Length];
        int instanceIndex = 0;
        foreach (var mesh in runtimeMeshes)
        {
            MeshInstances[instanceIndex++] = new MeshInstanceGPU
            {
                MeshIndex = meshDict[mesh.MeshDefinition.Id],
                Transform = MeshTransformGPU.Create(mesh.Transform.Matrix),
                MaterialIndex = mesh.MaterialIndex
            };
        }
        
        Globals.NumVertices = Vertices.Length;
        Globals.NumTriangles = Triangles.Length;
        Globals.NumMeshes = Meshes.Length;
        Globals.NumMeshInstances = MeshInstances.Length;
        
        DumpMeshesAndGeometry("Scene_MeshesAndGeometry_Dump.txt");
    }

    private void DumpMeshesAndGeometry(string fileName)
    {
        var sb = new StringBuilder(1 << 20);
        var ci = CultureInfo.InvariantCulture;

        sb.AppendLine("===== MESH / GEOMETRY DUMP =====");
        sb.AppendLine();

        for (int m = 0; m < Meshes.Length; m++)
        {
            var mesh = Meshes[m];

            sb.AppendLine($"--- Mesh {m} ---");
            sb.AppendLine($"VertexOffset   : {mesh.VertexOffset}");
            sb.AppendLine($"VertexCount    : {mesh.VertexCount}");
            sb.AppendLine($"TriangleOffset : {mesh.TriangleOffset}");
            sb.AppendLine($"TriangleCount  : {mesh.TriangleCount}");
            sb.AppendLine();

            sb.AppendLine("Vertices (global indices):");
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                int vi = mesh.VertexOffset + i;
                var v = Vertices[vi];

                sb.AppendLine(
                    $"  [{vi}] P=({v.Position.X.ToString(ci)}, {v.Position.Y.ToString(ci)}, {v.Position.Z.ToString(ci)}) " +
                    $"N=({v.Normal.X.ToString(ci)}, {v.Normal.Y.ToString(ci)}, {v.Normal.Z.ToString(ci)}) " +
                    $"UV=({v.UV.X.ToString(ci)}, {v.UV.Y.ToString(ci)})"
                );
            }

            sb.AppendLine();
            sb.AppendLine("Triangles:");
            for (int i = 0; i < mesh.TriangleCount; i++)
            {
                int ti = mesh.TriangleOffset + i;
                var t = Triangles[ti];

                int gv0 = mesh.VertexOffset + t.Vertex0;
                int gv1 = mesh.VertexOffset + t.Vertex1;
                int gv2 = mesh.VertexOffset + t.Vertex2;

                sb.AppendLine(
                    $"  [{ti}] local({t.Vertex0}, {t.Vertex1}, {t.Vertex2}) " +
                    $"-> global({gv0}, {gv1}, {gv2})"
                );

                sb.AppendLine(
                    $"       P0={Vertices[gv0].Position} " +
                    $"P1={Vertices[gv1].Position} " +
                    $"P2={Vertices[gv2].Position}"
                );
            }

            sb.AppendLine();
        }

        sb.AppendLine("Mesh Instances:");
        for (int i = 0; i < MeshInstances.Length; i++)
        {
            var inst = MeshInstances[i];
            sb.AppendLine(
                $"Instance {i}: MeshIndex={inst.MeshIndex}, MaterialIndex={inst.MaterialIndex}"
            );
            sb.AppendLine("  Transform:");
            sb.AppendLine($"    {inst.Transform.Row0}");
            sb.AppendLine($"    {inst.Transform.Row1}");
            sb.AppendLine($"    {inst.Transform.Row2}");
        }

        Dump.CreateDump(sb.ToString(), fileName);
    }
    private void CreateMaterials()
    {
        var materials = scene.Content.Materials;
        Materials = new MaterialGPU[materials.Material.Count];
        for (int i = 0; i < materials.Material.Count; i++)
        {
            Materials[i] = MaterialGPU.FromMaterial(materials.Material[i]);
        }
        
        Globals.NumMaterials = Materials.Length;
    }
}