using System.Runtime.CompilerServices;

namespace Raytracer.Core;

public static class HumanFormat
{
    private static readonly string[] Suffix =
        { "", "k", "M", "B", "T" }; // Thousand, Million, Billion, Trillion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(double value, string unit = "")
    {
        if (value == 0)
            return "0" + (unit != "" ? " " + unit : "");

        int mag = (int)Math.Floor(Math.Log10(Math.Abs(value)) / 3);  
        mag = Math.Clamp(mag, 0, Suffix.Length - 1);

        double scaled = value / Math.Pow(1000, mag);

        return $"{scaled:0.###}{Suffix[mag]}{(unit != "" ? " " + unit : "")}";
    }
}