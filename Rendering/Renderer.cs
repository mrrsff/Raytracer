using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.IO.ImageSavers;
using Raytracer.Rendering.Debug;
using Raytracer.Utility;

namespace Raytracer.Rendering
{
    public abstract class Renderer(Scene scene)
    {
        public Scene Scene { get; } = scene;
        protected Camera Camera { get; private set; } = null!;

        public ImageBuffer CreateEmptyImageBuffer(int cameraIndex)
        {
            Camera = Scene.GetCamera(cameraIndex);
            return new ImageBuffer(Camera.ImageResolution, ColorUtility.Black, Camera.ImageName);
        }

        public void RenderIntoExistingBuffer(int cameraIndex, ImageBuffer buffer)
        {
            Camera = Scene.GetCamera(cameraIndex);
            Camera.InitializeCamera();
            OnRender(buffer);
            DebugRenderer.Rasterize(Camera, buffer);
        }

        protected abstract void OnRender(ImageBuffer buffer);
    }
}