using System;
using Silk.NET.SDL;

namespace CatcherGame
{
    public unsafe class InputLogic
    {
        private readonly Sdl _sdl;
        private readonly GameLogic _gameLogic;
        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private bool _wasLeftDown = false;
        private bool _wasRightDown = false;
        private DateTimeOffset _lastLeftPressTime = DateTimeOffset.MinValue;
        private DateTimeOffset _lastRightPressTime = DateTimeOffset.MinValue;
        private const double DoubleTapThresholdMs = 250.0;

        public InputLogic(Sdl sdl, GameLogic gameLogic)
        {
            _sdl = sdl;
            _gameLogic = gameLogic;
        }

        public bool ProcessInput()
        {
            var currentTime = DateTimeOffset.Now;
            var timeSinceLastFrame = (currentTime - _lastUpdate).TotalMilliseconds;
            _lastUpdate = currentTime;

            Event ev = new Event();
            while (_sdl.PollEvent(ref ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit) return true;
            }

            int numKeys;
            byte* statePtr = _sdl.GetKeyboardState(&numKeys);
            var keyboardState = new ReadOnlySpan<byte>(statePtr, numKeys);

            bool isLeftDown = keyboardState[(int)Scancode.ScancodeLeft] == 1;
            bool isRightDown = keyboardState[(int)Scancode.ScancodeRight] == 1;

            // check for left dash
            if (isLeftDown && !_wasLeftDown)
            {
                if ((currentTime - _lastLeftPressTime).TotalMilliseconds < DoubleTapThresholdMs)
                {
                    _gameLogic.PlayerDash(-1.0);
                }
                _lastLeftPressTime = currentTime;
            }

            // check for right dash
            if (isRightDown && !_wasRightDown)
            {
                if ((currentTime - _lastRightPressTime).TotalMilliseconds < DoubleTapThresholdMs)
                {
                    _gameLogic.PlayerDash(1.0);
                }
                _lastRightPressTime = currentTime;
            }

            // save current state for the next frame
            _wasLeftDown = isLeftDown;
            _wasRightDown = isRightDown;
            
            double left = 0.0;
            double right = 0.0;
            

            if (keyboardState[(int)Scancode.ScancodeLeft] == 1) 
            {
                left = 1.0;
            }
            if (keyboardState[(int)Scancode.ScancodeRight] == 1) 
            {
                right = 1.0;
            }

            if (keyboardState[(int)Scancode.ScancodeSpace] == 1 || keyboardState[(int)Scancode.ScancodeUp] == 1)
            {
                _gameLogic.PlayerJump();
            }

            _gameLogic.UpdatePlayerPosition(left, right, timeSinceLastFrame);

            return false;
        }
    }
}