using System;
using Silk.NET.SDL;

namespace CatcherGame
{
    public unsafe class GameWindow
    {
        private readonly Sdl _sdl;
        private readonly IntPtr _window;

        public GameWindow(Sdl sdl)
        {
            _sdl = sdl;
            _window = (IntPtr)sdl.CreateWindow(
                "Dog & Treat Catcher", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 640, 400,
                (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
            );

            if (_window == IntPtr.Zero) throw new Exception("Failed to create window.");
        }

        public IntPtr CreateRenderer()
        {
            return (IntPtr)_sdl.CreateRenderer((Window*)_window, -1, (uint)RendererFlags.Accelerated);
        }

        public (int Width, int Height) Size
        {
            get
            {
                int width = 0;
                int height = 0;
                _sdl.GetWindowSize((Window*)_window, ref width, ref height);
                return (width, height);
            }
        }

        public void Destroy()
        {
            _sdl.DestroyWindow((Window*)_window);
        }
    }
}