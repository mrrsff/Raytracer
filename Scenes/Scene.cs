using System.Drawing;
using System.Numerics;

namespace Raytracer.Scenes;

public class Scene
{
    public SceneContent Content;
    
    public Scene(SceneContent content)
    {
        Content = content;
    }
}