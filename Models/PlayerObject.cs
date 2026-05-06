using System;
using System.IO;
using Silk.NET.SDL;
using Silk.NET.Maths;

namespace CatcherGame.Models
{
    public class PlayerObject : GameObject
    {
        public double X { get; set; } = 300;
        public double Y { get; set; } = 320; 
        
        public Rectangle<int> TextureDestination { get; private set; }
        private Rectangle<int> _source = new(0, 0, 48, 48);
        
        private readonly int _textureId;
        private readonly int _jumpingTextureId;
        
        private const int BaseSpeed = 200; 
        public double SpeedMultiplier { get; set; } = 1.0; 

        private bool _isJumping = false;
        private double _yVelocity = 0;
        private const double Gravity = 1200.0;
        private const double JumpPower = -500.0;
        private const int GroundLevel = 320;

        private bool _isDashing = false;
        private double _dashTimer = 0;
        private double _dashDirection = 0; 
        private const double DashDurationMs = 200.0;
        private const double DashSpeedMultiplier = 2.5;

        public PlayerObject(GameRenderer renderer) : base()
        {
            _textureId = renderer.LoadTexture(Path.Combine("Assets", "dog.png"), out _);
            _jumpingTextureId = renderer.LoadTexture(Path.Combine("Assets", "dog_jump.png"), out _);
            
            if (_textureId < 0 || _jumpingTextureId < 0) 
                throw new Exception("Failed to load dog textures");
                
            UpdateTarget();
        }

        public void Jump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _yVelocity = JumpPower;
            }
        }

        public void Dash(double direction)
        {
            if (!_isDashing && direction != 0)
            {
                _isDashing = true;
                _dashTimer = DashDurationMs;
                _dashDirection = direction;
            }
        }

        public void UpdatePosition(double left, double right, double msSinceLastFrame, int screenWidth)
        {
            double dt = msSinceLastFrame / 1000.0;

            var pixelsToMove = BaseSpeed * SpeedMultiplier * dt;

            // horizontal movement
            if (_isDashing)
            {
                _dashTimer -= msSinceLastFrame;
                if (_dashTimer <= 0)
                {
                    _isDashing = false; // Dash is over
                }
                else
                {
                    // we override normal movement with the dash
                    X += (pixelsToMove * DashSpeedMultiplier * _dashDirection);
                }
            }
            else
            {
                X -= (pixelsToMove * left);
                X += (pixelsToMove * right);
            }

            if (X < 0) X = 0;
            if (X > screenWidth - 48) X = screenWidth - 48;

            // vertical movement
            if (_isJumping)
            {
                _yVelocity += Gravity * dt;
                Y += _yVelocity * dt;

                if (Y >= GroundLevel)
                {
                    Y = GroundLevel;
                    _isJumping = false;
                    _yVelocity = 0;
                }
            }

            UpdateTarget();
        }

        private void UpdateTarget()
        {
            TextureDestination = new Rectangle<int>((int)X, (int)Y, 48, 48);
        }

        public void Render(GameRenderer renderer)
        {
            int activeTexture = _isJumping ? _jumpingTextureId : _textureId;
            
            renderer.RenderTexture(activeTexture, _source, TextureDestination);
        }
    }
}