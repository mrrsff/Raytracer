namespace Raytracer.Rendering.Vulkan;

public class TimeManager
{
    public static float TotalTime;
    public static float DeltaTime { get; private set; }
    private static DateTime _lastFrameTime;

    public void Update(double dt) => Update((float)dt);
    public void Update(float dt)
    {
        if (dt > 0)
        {
            DeltaTime = dt;
            TotalTime += dt;
        }
    }
    
    public void Update()
    {
        var currentTime = DateTime.Now;
        if (_lastFrameTime == default)
        {
            _lastFrameTime = currentTime;
            DeltaTime = 0;
            return;
        }

        DeltaTime = (float)(currentTime - _lastFrameTime).TotalSeconds;
        TotalTime += DeltaTime;
        _lastFrameTime = currentTime;
    }
}