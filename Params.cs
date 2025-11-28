namespace Raytracer;

public static class Params
{
    public static string ScenePath;
    public static string SceneDirectory;
    public static bool IsSingleFile => File.Exists(ScenePath);
    public static bool IsDirectory => Directory.Exists(ScenePath);
    public static bool EnablePreview;
    
    public static void FromArgs(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: ./raytracer scene.json [--preview]");
        }

        ScenePath = args[0];
        bool isSingleFile = File.Exists(ScenePath);
        bool isDirectory = Directory.Exists(ScenePath);
        if (isSingleFile)
        {
            SceneDirectory = Path.GetDirectoryName(ScenePath) ?? "";
        }
        else if (isDirectory)
        {
            SceneDirectory = ScenePath;
        }

        var enablePreview = !args.Contains("--no-preview");

        EnablePreview = enablePreview;
    }
}