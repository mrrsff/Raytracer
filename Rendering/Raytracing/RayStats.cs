using System.Diagnostics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Debug = Raytracer.Core.Debug;

namespace Raytracer.Rendering.Raytracing;

public static class RayStats
{
    public static long Primary;
    public static long Shadow;

    public static long Total => Primary + Shadow;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reset()
    {
        Primary = 0;
        Shadow = 0;
    }

    public static float GetThroughput(long renderTimeInTicks)
    {
        if (renderTimeInTicks == 0) return 0;
        return Total / (float)renderTimeInTicks;
    }
    
    // Thread safe increment methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IncrementPrimary()
    {
        if (!Debug.MeasureRayThroughput) return;
        Interlocked.Increment(ref Primary);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IncrementShadow()
    {
        if (!Debug.MeasureRayThroughput) return;
        Interlocked.Increment(ref Shadow);
    }
    public static void ThroughputMonitor(TimeSpan interval, CancellationToken token)
    {
        if (!Debug.MeasureRayThroughput) return;
        Debug.Log("Measuring ray throughput...");
        var sw = Stopwatch.StartNew();
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(interval, token);

                long current = Total;
                double elapsedSec = sw.Elapsed.TotalSeconds;
                double raysPerSecond = elapsedSec > 0 ? current / elapsedSec : 0.0;
                string formattedThroughput = HumanFormat.Format(raysPerSecond, "rays/s");
                Debug.Log($"{DateTime.Now:HH:mm:ss} : avg. {formattedThroughput}");
            }
        }, token);
        
        token.Register(() =>
        {
            long current = Total;
            double elapsedSec = sw.Elapsed.TotalSeconds;
            double raysPerSecond = elapsedSec > 0 ? current / elapsedSec : 0.0;
            string formattedThroughput = HumanFormat.Format(raysPerSecond, "rays/s");
            Debug.Log($"Final throughput: avg. {formattedThroughput}");
        });
    }
    
    
}