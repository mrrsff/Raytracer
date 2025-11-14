namespace Raytracer;

public struct Params
{
    public string ScenePath;
    public bool EnablePreview;
    
    public static Params FromArgs(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: ./raytracer scene.json [--preview]");
        }

        var scenePath = args[0];
        var enablePreview = args.Contains("--preview");

        return new Params
        {
            ScenePath = scenePath,
            EnablePreview = enablePreview
        };
    }
}