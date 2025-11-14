using System.Numerics;
using static SDL2.SDL;

namespace Raytracer.Rendering.SDL2;

public class SDLPreview : SDLWindow
{
    private IntPtr texture;

    private float zoom = 1.0f;
    private const float minZoom = 0.1f;
    private const float maxZoom = 15.0f;

    private Vector2 panOffset = Vector2.Zero;
    private bool isPanning = false;
    private Vector2 lastMousePos = Vector2.Zero;

    public SDLPreview(int width, int height)
        : base(width, height, "Raytracer Preview")
    {
        CreateTexture(width, height);
    }

    private void CreateTexture(int w, int h)
    {
        if (texture != IntPtr.Zero)
            SDL_DestroyTexture(texture);

        texture = SDL_CreateTexture(
            renderer,
            SDL_PIXELFORMAT_ABGR8888,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            w, h
        );
    }

    public void UpdateFrame(byte[] pixelData)
    {
        // *** VALIDATE BUFFER SIZE ***
        int expected = contentWidth * contentHeight * 4;
        if (pixelData.Length != expected)
        {
            throw new Exception(
                $"Invalid pixel buffer size: expected {expected}, got {pixelData.Length}"
            );
        }

        unsafe
        {
            IntPtr pixels;
            int pitch;
            SDL_LockTexture(texture, IntPtr.Zero, out pixels, out pitch);

            fixed (byte* src = pixelData)
            {
                byte* dst = (byte*)pixels;

                int srcRowBytes = contentWidth * 4;

                if (pitch < srcRowBytes)
                {
                    SDL_UnlockTexture(texture);
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

            SDL_UnlockTexture(texture);
        }

        SDL_RenderClear(renderer);

        SDL_Rect destinationRect = ComputeDestinationRect();

        SDL_RenderCopy(renderer, texture, IntPtr.Zero, ref destinationRect);
        SDL_RenderPresent(renderer);
    }

    private SDL_Rect ComputeDestinationRect()
    {
        float texAspect = (float)contentWidth / contentHeight;
        float winAspect = (float)winWidth / winHeight;

        int baseW, baseH;

        if (winAspect > texAspect)
        {
            baseH = winHeight;
            baseW = (int)(winHeight * texAspect);
        }
        else
        {
            baseW = winWidth;
            baseH = (int)(winWidth / texAspect);
        }

        int dstW = (int)(baseW * zoom);
        int dstH = (int)(baseH * zoom);

        SDL_Rect r;
        r.w = dstW;
        r.h = dstH;

        r.x = (winWidth - dstW) / 2;
        r.y = (winHeight - dstH) / 2;

        r.x += (int)panOffset.X;
        r.y += (int)panOffset.Y;

        return r;
    }

    protected override void MouseWheelEvent(int yDelta)
    {
        SDL_GetMouseState(out int mx, out int my);

        float newZoom = zoom;
        if (yDelta > 0)
            newZoom *= 1.1f;
        else if (yDelta < 0)
            newZoom /= 1.1f;

        ZoomAtCursor(newZoom, mx, my);
    }

    protected override void KeyDownEvent(SDL_KeyboardEvent keyEvent)
    {
        if (keyEvent.keysym.sym == SDL_Keycode.SDLK_r) 
        {
            zoom = 1.0f;
        }
        
        if (keyEvent.keysym.sym == SDL_Keycode.SDLK_ESCAPE) 
        {
            SDL_Event quitEvent = new SDL_Event
            {
                type = SDL_EventType.SDL_QUIT
            };
            SDL_PushEvent(ref quitEvent);
        }
        
        if (keyEvent.keysym.sym == SDL_Keycode.SDLK_SPACE) 
        {
            ResetPan();
        }
    }

    protected override void MouseButtonEvent(SDL_MouseButtonEvent buttonEvent)
    {
        if (buttonEvent.button == SDL_BUTTON_RIGHT)
        {
            if (buttonEvent.state == SDL_PRESSED)
            {
                isPanning = true;
                lastMousePos = new Vector2(buttonEvent.x, buttonEvent.y);
            }
            else if (buttonEvent.state == SDL_RELEASED)
            {
                isPanning = false;
            }
        }
    }

    protected override void MouseMotionEvent(SDL_MouseMotionEvent motionEvent)
    {
        if (isPanning)
        {
            Vector2 currentMousePos = new Vector2(motionEvent.x, motionEvent.y);
            Vector2 delta = currentMousePos - lastMousePos;

            panOffset += delta / zoom * zoom;

            lastMousePos = currentMousePos;
        }
    }

    private void ResetPan()
    {
        panOffset = Vector2.Zero;
    }
    
    private void ZoomAtCursor(float newZoom, int mouseX, int mouseY)
    {
        newZoom = Math.Clamp(newZoom, minZoom, maxZoom);
        if (Math.Abs(newZoom - zoom) < 0.0001f)
            return;

        Vector2 beforeZoomMouseWorld = (new Vector2(mouseX, mouseY) - new Vector2(winWidth, winHeight) / 2 - panOffset) / zoom;
        zoom = newZoom;
        Vector2 afterZoomMouseWorld = (new Vector2(mouseX, mouseY) - new Vector2(winWidth, winHeight) / 2 - panOffset) / zoom;

        panOffset += (afterZoomMouseWorld - beforeZoomMouseWorld) * zoom;
    }
}