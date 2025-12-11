using Raytracer.Core;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.IO.Images;
using Raytracer.Rendering.DebugRendering;
using Raytracer.Utility;

namespace Raytracer.Rendering
{
    public abstract class Renderer(Scene scene)
    {
        public Scene Scene { get; } = scene;
        public Camera Camera { get; private set; } = null!;

        public ImageBuffer CreateEmptyImageBuffer(int cameraIndex)
        {
            Camera = Scene.GetCamera(cameraIndex);
            return new ImageBuffer(Camera.ImageResolution, ColorUtility.Black, Camera.ImageName);
        }

        public void RenderIntoExistingBuffer(int cameraIndex, ImageBuffer buffer)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            if (Debug.PrintRenderTime) Debug.Log($"Starting render for camera '{Scene.GetCamera(cameraIndex).ImageName}'...");
            Camera = Scene.GetCamera(cameraIndex);
            Camera.InitializeCamera();
            OnRender(buffer);
            DebugRenderer.Rasterize(Camera, buffer);
            stopwatch.Stop();
            if (Debug.PrintRenderTime) Debug.Log($"Render completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }

        protected abstract void OnRender(ImageBuffer buffer);
    }
}