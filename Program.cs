using System.Threading;
using Silk.NET.SDL;

namespace CatcherGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var sdl = Sdl.GetApi();
            if (sdl.Init(Sdl.InitVideo | Sdl.InitEvents) < 0)
            {
                throw new System.Exception("Failed to initialize SDL.");
            }

            var gameWindow = new GameWindow(sdl);
            var gameRenderer = new GameRenderer(sdl, gameWindow);
            var gameLogic = new GameLogic(gameRenderer);
            var inputLogic = new InputLogic(sdl, gameLogic);

            gameLogic.InitializeGame();

            bool quit = false;
            while (!quit)
            {
                quit = inputLogic.ProcessInput();
                if (quit) break;
                
                gameLogic.RenderFrame();
                
                System.Threading.Thread.Sleep(13); // 75ish FPS
            }

            gameWindow.Destroy();
            sdl.Quit();
            
            gameLogic.StopMusic();

            System.Environment.Exit(0);
        }
    }
}