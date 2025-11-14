using static SDL2.SDL;

namespace Raytracer.Rendering.SDL2;

public class SDLPreview
{
    private IntPtr window;
    private IntPtr renderer;
    private IntPtr texture;
    private int width, height;

    public SDLPreview(int width, int height)
    {
        this.width = width;
        this.height = height;

        SDL_Init(SDL_INIT_VIDEO);

        window = SDL_CreateWindow(
            "Raytracer Preview",
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            width, height,
            SDL_WindowFlags.SDL_WINDOW_SHOWN
        );

        renderer = SDL_CreateRenderer(
            window,
            -1,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED
        );

        texture = SDL_CreateTexture(
            renderer,
            SDL_PIXELFORMAT_ABGR8888,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            width,
            height
        );
    }

    public void UpdateFrame(byte[] pixelData)
    {
        unsafe
        {
            IntPtr pixels;
            int pitch;
            SDL_LockTexture(texture, IntPtr.Zero, out pixels, out pitch);

            fixed (byte* srcBase = pixelData)
            {
                byte* dstBase = (byte*)pixels;
                int srcRowBytes = width * 4; // RGBA8888

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcBase + y * srcRowBytes;
                    byte* dstRow = dstBase + y * pitch;

                    Buffer.MemoryCopy(srcRow, dstRow, pitch, srcRowBytes);
                }
            }

            SDL_UnlockTexture(texture);
        }

        SDL_RenderClear(renderer);
        SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
        SDL_RenderPresent(renderer);
    }
    
    public bool PollEvents()
    {
        while (SDL_PollEvent(out SDL_Event e) != 0)
        {
            if (e.type == SDL_EventType.SDL_QUIT)
                return false;
        }
        return true;
    }
}