using Raytracer.IO.SceneLoaders;

namespace Raytracer.Scenes;

public static class SceneProvider
{
    public static IEnumerable<Scene> GetScenes(string path)
    {
        if (File.Exists(path))
        {
            yield return SceneLoader.Load(path);
        }
        else if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                yield return SceneLoader.Load(file);
            }
        }
        else
        {
            throw new FileNotFoundException("Scene path not found: " + path);
        }
    }
}