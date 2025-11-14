using System.Runtime.CompilerServices;

namespace Raytracer.Rendering.Sampling;

public static class ThreadRng
{
    [ThreadStatic] private static XorShift128Plus? _local;

    private static XorShift128Plus Instance =>
        _local ??= new XorShift128Plus(SeedBase + (ulong)Thread.CurrentThread.ManagedThreadId * 0x9E3779B97F4A7C15UL);

    private const ulong SeedBase = 0x1234ABCD5678EF90UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NextDouble() => Instance.NextDouble();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NextFloat() => (float)Instance.NextDouble();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextInt(int max) => Instance.NextInt(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextInt(int min, int max) => min + Instance.NextInt(max - min);

    private sealed class XorShift128Plus
    {
        private ulong _s0, _s1;

        public XorShift128Plus(ulong seed)
        {
            // SplitMix64 seeding
            ulong z = seed + 0x9E3779B97F4A7C15UL;
            _s0 = Mix(z);
            z += 0x9E3779B97F4A7C15UL;
            _s1 = Mix(z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Mix(ulong z)
        {
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextULong()
        {
            ulong x = _s0;
            ulong y = _s1;
            _s0 = y;
            x ^= x << 23;
            _s1 = x ^ y ^ (x >> 17) ^ (y >> 26);
            return _s1 + y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() =>
            (NextULong() >> 11) * (1.0 / (1UL << 53));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int max) =>
            (int)(NextULong() % (uint)max);
    }
}