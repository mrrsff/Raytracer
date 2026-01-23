using Raytracer.Rendering.Sampling;

namespace Raytracer.Utility;

public static class ListExtensions
{
    private static T GetRandomElement<T>(this IList<T> list, float randomValue)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentException("The list cannot be null or empty.");

        int index = (int)(randomValue * list.Count);
        if (index == list.Count) index = list.Count - 1; // Handle edge case
        return list[index];
    }
    
    public static T GetRandomElement<T>(this IList<T> list)
    {
        return GetRandomElement(list, ThreadRng.NextInt(list.Count));
    }
}