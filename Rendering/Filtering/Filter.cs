using System.Runtime.CompilerServices;

namespace Raytracer.Rendering.Filtering;

public static class Filter 
{
    public class Gaussian
    {
        public static float Evaluate(float x, float y)
        {
            const float sigma = 0.5f;
            const float radius = sigma * 2;
            if (MathF.Abs(x) > radius || MathF.Abs(y) > radius)
                return 0f;

            float gX = Gaussian1D(x, sigma);
            float gY = Gaussian1D(y, sigma);

            return gX * gY;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Gaussian1D(float x, float sigma)
        {
            float inv2Sigma2 = -0.5f / (sigma * sigma);
            return MathF.Exp(x * x * inv2Sigma2);
        }
    }
    
}