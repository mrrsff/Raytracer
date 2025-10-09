using System.Text.Json.Serialization;

namespace Raytracer.Core;

public enum MaterialType
{
    None,
    Mirror,
    Conductor,
    Dielectric
}