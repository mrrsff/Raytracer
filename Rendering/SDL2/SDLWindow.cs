using System.Numerics;
using static SDL2.SDL;

namespace Raytracer.Rendering.SDL2;

public abstract class SDLWindow
{
    protected IntPtr window;
    protected IntPtr renderer;

    protected int winWidth;
    protected int winHeight;

    protected int contentWidth;
    protected int contentHeight;

    public SDLWindow(int width, int height, string title)
    {
        contentWidth = width;
        contentHeight = height;

        SDL_Init(SDL_INIT_VIDEO);

        winWidth = width;
        winHeight = height;

        window = SDL_CreateWindow(
            title,
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            winWidth,
            winHeight,
            SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL_WindowFlags.SDL_WINDOW_RESIZABLE
        );

        renderer = SDL_CreateRenderer(
            window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC |
            SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE
        );
    }

    public virtual void Resize(int newW, int newH)
    {
        winWidth = newW;
        winHeight = newH;
    }

    protected abstract void MouseWheelEvent(int yDelta);

    public bool PollEvents()
    {
        while (SDL_PollEvent(out SDL_Event e) != 0)
        {
            switch (e.type)
            {
                case SDL_EventType.SDL_QUIT:
                    return false;

                case SDL_EventType.SDL_WINDOWEVENT:
                    switch (e.window.windowEvent)
                    {
                        case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                        case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            Resize(e.window.data1, e.window.data2);
                            break;
                    }
                    break;

                case SDL_EventType.SDL_MOUSEWHEEL:
                    MouseWheelEvent(e.wheel.y);
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    MouseMotionEvent(e.motion);
                    break;

                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    MouseButtonEvent(e.button);
                    break;

                case SDL_EventType.SDL_KEYDOWN:
                    KeyDownEvent(e.key);
                    break;
            }
        }

        return true;
    }


    protected abstract void KeyDownEvent(SDL_KeyboardEvent keyEvent);

    protected abstract void MouseMotionEvent(SDL_MouseMotionEvent motionEvent);
    protected abstract void MouseButtonEvent(SDL_MouseButtonEvent buttonEvent);
}
