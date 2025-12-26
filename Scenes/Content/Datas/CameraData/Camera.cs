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

public enum CameraType { None, LookAt }
public enum Handedness { right, left }

public class Camera
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public CameraType Type;
    public Vector3 Position;
    public Vector3 Gaze;
    public Vector3 GazePoint;
    public Handedness _handedness = Handedness.right;
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

    private Tonemapper[] runtimeTonemaps;

    private float ApertureSize;
    private float FocusDistance;
    private float ShutterOpen;
    private float ShutterClose = 1.0f;

    public Transform Transform = new Transform();
    public void Initialize()
    {
        Position = Transform.ToWorldPoint(Position);
        Gaze = Transform.ToWorldDirection(Gaze);
        Up = Transform.ToWorldDirection(Up);
        
        float aspect = (float)ImageResolution.Width / ImageResolution.Height;
        if (Type == CameraType.LookAt)
        {
            Gaze = Vector3.Normalize(GazePoint - Position);

            NearPlane.Top = NearDistance * MathF.Tan(FovY * MathF.PI / 360.0f);
            NearPlane.Bottom = -NearPlane.Top;
            NearPlane.Right = aspect * NearPlane.Top;
            NearPlane.Left = -NearPlane.Right;
        }
        else
        {
            FovY = 2.0f * MathF.Atan(NearPlane.Top / NearDistance) * 180.0f / MathF.PI;
        }

        var w = Vector3.Normalize(-Gaze);
        var v = Vector3.Normalize(Up - Vector3.Dot(Up, w) * w);
        Vector3 u = _handedness == Handedness.right ? Vector3.Cross(v, w) : Vector3.Cross(w, v);

        Forward = -w; // points into scene
        Right = u;
        Up = v;

        M = Position + Forward * NearDistance;
        Q = M + (NearPlane.Left * Right) + (NearPlane.Top * Up);

        SUMultiplier = (NearPlane.Right - NearPlane.Left) / ImageResolution.Width;
        SVMultiplier = (NearPlane.Top - NearPlane.Bottom) / ImageResolution.Height;

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
    public Vector3 Q { get; private set; }
    public Vector3 M { get; private set; }
    public float SUMultiplier { get; private set; }
    public float SVMultiplier { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GenerateRay(float i, float j)
    {
        float sU = i * SUMultiplier;
        float sV = j * SVMultiplier;

        Vector3 s = Q + (sU * Right) - (sV * Up);
        Vector3 d = Vector3.Normalize(s - Position);
        return new Ray(Position, d);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GenerateRayDRT(float pixelX, float pixelY, Vector2 lensSample = default, float time = 0.0f)
    {
        float sU = pixelX * SUMultiplier;
        float sV = pixelY * SVMultiplier;

        Vector3 s = Q + (sU * Right) - (sV * Up);

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
        ImageBuffer[] tonemappedImages = new ImageBuffer[runtimeTonemaps.Length + 1];
        tonemappedImages[^1] = image; // Original image
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
                    // if (i > 0)
                    //     Debug.Log($"Tonemapping [{i}] Pixel ({x}, {y}): Original Color {image.GetPixel(x, y)} -> Tonemapped Color {tonemappedColor}");
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