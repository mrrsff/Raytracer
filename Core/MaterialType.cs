using System.Text.Json.Serialization;

namespace Raytracer.Core;

public enum MaterialType
{
    None = 0,
    Mirror,
    Conductor,
    Dielectric
}