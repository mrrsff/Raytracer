using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Sampling;

public static class Sampler
{
    public static class MultiJittered
    {
        public static Vector2[] Sample(int n)
        {
            int sqrtN = (int)Math.Sqrt(n);
            Vector2[] samples = new Vector2[n];

            float subcellWidth = 1.0f / n;

            // Initialize the samples to the center of each subcell
            for (int i = 0; i < sqrtN; i++)
            {
                for (int j = 0; j < sqrtN; j++)
                {
                    int index = i * sqrtN + j;
                    samples[index] = new Vector2(
                        (i * sqrtN + j) * subcellWidth + subcellWidth / 2,
                        (j * sqrtN + i) * subcellWidth + subcellWidth / 2
                    );
                }
            }

            // Shuffle x coordinates within each row
            Random rand = new Random();
            for (int i = 0; i < sqrtN; i++)
            {
                for (int j = 0; j < sqrtN; j++)
                {
                    int k = rand.Next(j, sqrtN);
                    int index1 = i * sqrtN + j;
                    int index2 = i * sqrtN + k;

                    (samples[index1].X, samples[index2].X) = (samples[index2].X, samples[index1].X);
                }
            }

            // Shuffle y coordinates within each column
            for (int j = 0; j < sqrtN; j++)
            {
                for (int i = 0; i < sqrtN; i++)
                {
                    int k = rand.Next(i, sqrtN);
                    int index1 = i * sqrtN + j;
                    int index2 = k * sqrtN + j;

                    (samples[index1].Y, samples[index2].Y) = (samples[index2].Y, samples[index1].Y);
                }
            }

            return samples;
        }
    }
    public static Vector2[] Uniform(int n)
    {
        Vector2[] samples = new Vector2[n];
        float invN = 1.0f / n;

        for (int i = 0; i < n; i++)
        {
            samples[i] = new Vector2((i + 0.5f) * invN, (i + 0.5f) * invN);
        }

        return samples;
    }
    public static float[] OneDimensionalUniform(int n)
    {
        float[] samples = new float[n];
        float invN = 1.0f / n;

        for (int i = 0; i < n; i++)
        {
            samples[i] = (i + 0.5f) * invN;
        }

        return samples;
    }
}