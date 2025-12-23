using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Rendering.CPU.Tonemaps;
using Raytracer.Utility;

namespace Raytracer.Scenes.Content.Datas.CameraData;

public enum CameraType
{
    None,
    LookAt
}

public class Camera
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public CameraType Type;
    public Vector3 Position;
    public Vector3 Gaze;
    public Vector3 GazePoint;
    public Vector3 Up;
    public float FovY;
    public Rect NearPlane;
    public float NearDistance = 1.0f;
    public int NumSamples = 1;
    public Resolution ImageResolution;
    public string ImageName;
    public string Transformations;
    
    [JsonConverter(typeof(SingleOrListConverter<TonemapData>))]
    public List<TonemapData> Tonemap;

    public Tonemapper[] runtimeTonemaps;
    
    public float ApertureSize = 0;
    public float FocusDistance = 0f;
    public float ShutterOpen = 0.0f;
    public float ShutterClose = 1.0f;
    
    public Transform Transform = new Transform();
    public void InitializeCamera()
    {
        // Transform position and orientation
        Position = Transform.ToWorldPoint(Position);
        Gaze = Transform.ToWorldDirection(Gaze);
        Up = Transform.ToWorldDirection(Up);
        
        if (Type == CameraType.LookAt)
        {
            Gaze = Vector3.Normalize(GazePoint - Position);

            float aspect = (float)ImageResolution.Width / ImageResolution.Height;
            NearPlane.Top = NearDistance * MathF.Tan(FovY * MathF.PI / 360.0f);
            NearPlane.Bottom = -NearPlane.Top;
            NearPlane.Right = aspect * NearPlane.Top;
            NearPlane.Left = -NearPlane.Right;
        }

        var w = Vector3.Normalize(-Gaze);
        var v = Vector3.Normalize(Up - Vector3.Dot(Up, w) * w);
        var u = Vector3.Cross(v, w);

        Forward = -w; // points into scene
        Right = u;
        Up = v;

        m = Position + Forward * NearDistance;
        q = m + (NearPlane.Left * Right) + (NearPlane.Top * Up);

        sUMultiplier = (NearPlane.Right - NearPlane.Left) / ImageResolution.Width;
        sVMultiplier = (NearPlane.Top - NearPlane.Bottom) / ImageResolution.Height;

        if (Tonemap == null) return;
        
        runtimeTonemaps = new Tonemapper[Tonemap.Count];
        for (var i = 0; i < Tonemap.Count; i++)
        {
            var tonemapData = Tonemap[i];
            runtimeTonemaps[i] = TonemapUtility.CreateTonemapFromData(tonemapData);
        }
    }

    public Vector3 Forward;
    public Vector3 Right;
    public Vector3 Q => q;
    public Vector3 M => m;
    public float SUMultiplier => sUMultiplier;
    public float SVMultiplier => sVMultiplier;
    
    private Vector3 m;
    private Vector3 q;
    private float sUMultiplier;
    private float sVMultiplier;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GenerateRay(float i, float j)
    {
        float sU = i * sUMultiplier;
        float sV = j * sVMultiplier;

        Vector3 s = q + (sU * Right) - (sV * Up);
        Vector3 d = Vector3.Normalize(s - Position);
        return new Ray(Position, d);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GenerateRayDRT(float pixelX, float pixelY, Vector2 lensSample = default, float time = 0.0f)
    {
        float sU = pixelX * sUMultiplier;
        float sV = pixelY * sVMultiplier;

        Vector3 s = q + (sU * Right) - (sV * Up);

        Vector3 dir = Vector3.Normalize(s - Position);

        if (ApertureSize <= 0.0f)
        {
            var r = new Ray(Position, dir);
            r.Time = MathUtility.Lerp(ShutterOpen, ShutterClose, time);
            return r;
        }
        
        Vector3 focalPoint = Position + dir * FocusDistance;

        Vector2 disk = MathUtility.ConcentricDiskSample(lensSample) * ApertureSize;
        Vector3 lensPos = Position + disk.X * Right + disk.Y * Up;

        Vector3 newDir = Vector3.Normalize(focalPoint - lensPos);

        var ray = new Ray(lensPos, newDir);
        ray.Time = MathUtility.Lerp(ShutterOpen, ShutterClose, time);
        return ray;
    }
    
    public ImageBuffer[] ApplyTonemaps(ImageBuffer image)
    {
        if (runtimeTonemaps == null || runtimeTonemaps.Length == 0) return [image];
        ImageBuffer[] tonemappedImages = new ImageBuffer[runtimeTonemaps.Length];
        for (int i = 0; i < runtimeTonemaps.Length; i++)
        {
            tonemappedImages[i] = new ImageBuffer(image.Width, image.Height, Vector3.Zero);
            var tonemap = runtimeTonemaps[i];
            
            var tonemapExtensionIndex = Tonemap[i].Extension;
            var filename = Path.GetFileNameWithoutExtension(image.OutputName);
            tonemappedImages[i].OutputName = $"{filename}{tonemapExtensionIndex}";
            
            tonemap.Prepare(image);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Vector3 tonemappedColor = tonemap.Apply(image, x, y);
                    tonemappedImages[i].SetPixel(x, y, tonemappedColor);
                }
            }
        }
        return tonemappedImages;
    }
    
    public ImageBuffer ApplyTonemapSingle(ImageBuffer image, int tonemapIndex = 0)
    {
        if (runtimeTonemaps == null || runtimeTonemaps.Length == 0) return image;
        tonemapIndex = Math.Clamp(tonemapIndex, 0, runtimeTonemaps.Length - 1);

        ImageBuffer tonemappedImage = new ImageBuffer(image.Width, image.Height, Vector3.Zero);
        var tonemap = runtimeTonemaps[tonemapIndex];
        tonemap.Prepare(image);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Vector3 tonemappedColor = tonemap.Apply(image, x, y);
                tonemappedImage.SetPixel(x, y, tonemappedColor);
            }
        }

        return tonemappedImage;
    }
    
    public override string ToString()
    {
        return new StringBuilder().Append("Camera(Id: ")
            .Append(Id)
            .Append(", Position: ")
            .Append(Position)
            .Append(", Gaze: ")
            .Append(Gaze)
            .Append(", Up: ")
            .Append(Up)
            .Append(", NearPlane: ")
            .Append(NearPlane)
            .Append(", NearDistance: ")
            .Append(NearDistance)
            .Append(", NumSamples: ")
            .Append(NumSamples)
            .Append(", ImageResolution: ")
            .Append(ImageResolution)
            .Append(", ImageName: ")
            .Append(ImageName)
            .Append(", Transformations: ")
            .Append(Transformations)
            .Append(", Tonemap: ")
            .Append(Tonemap)
            .Append(')')
            .ToString();
    }
}