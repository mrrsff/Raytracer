using System.Numerics;
using static SDL2.SDL;

namespace Raytracer.Rendering.SDL2;

public abstract class SDLWindow
{
    protected IntPtr renderer;
    protected IntPtr window;
    protected int winWidth;
    protected int winHeight;

    protected IntPtr contentTexture;
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
        
        contentTexture = CreateTexture(contentWidth, contentHeight);
    }
    
    private IntPtr CreateTexture(int w, int h)
    {
        return SDL_CreateTexture(
            renderer,
            SDL_PIXELFORMAT_ARGB8888,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w,
            h
        );
    }

    public virtual void Resize(int newW, int newH)
    {
        winWidth = newW;
        winHeight = newH;
    }
    public void SetContentDimensions(int w, int h)
    {
        contentWidth = w;
        contentHeight = h;

        SDL_DestroyTexture(contentTexture);
        contentTexture = CreateTexture(contentWidth, contentHeight);
    }
    public void UpdateFrame(byte[] pixelData, int w, int h)
    {
        if (w != contentWidth || h != contentHeight)
        {
            contentWidth = w;
            contentHeight = h;

            SDL_DestroyTexture(contentTexture);
            contentTexture = CreateTexture(contentWidth, contentHeight);
        }
        
        unsafe
        {
            IntPtr pixels;
            int pitch;
            SDL_LockTexture(contentTexture, IntPtr.Zero, out pixels, out pitch);

            fixed (byte* src = pixelData)
            {
                byte* dst = (byte*)pixels;

                int srcRowBytes = contentWidth * 4;

                if (pitch < srcRowBytes)
                {
                    SDL_UnlockTexture(contentTexture);
                    throw new Exception(
                        $"SDL pitch smaller than row size ({pitch} < {srcRowBytes})."
                    );
                }

                for (int y = 0; y < contentHeight; y++)
                {
                    byte* srcRow = src + y * srcRowBytes;
                    byte* dstRow = dst + y * pitch;

                    Buffer.MemoryCopy(srcRow, dstRow, pitch, srcRowBytes);
                }
            }

            SDL_UnlockTexture(contentTexture);
        }

        SDL_RenderClear(renderer);

        SDL_Rect destinationRect = ComputeDestinationRect();

        SDL_RenderCopy(renderer, contentTexture, IntPtr.Zero, ref destinationRect);
        SDL_RenderPresent(renderer);
    }

    protected virtual SDL_Rect ComputeDestinationRect()
    {
        return new SDL_Rect
        {
            x = 0,
            y = 0,
            w = winWidth,
            h = winHeight
        };
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
