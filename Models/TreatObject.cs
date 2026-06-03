using System;
using System.IO;
using Silk.NET.SDL;
using Silk.NET.Maths;

namespace CatcherGame.Models
{
    public enum ItemType 
    { 
        Treat, 
        Choco 
    }

    public class TreatObject : GameObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Rectangle<int> TextureDestination { get; private set; }

        public ItemType Type { get; private set; }
        
        private Rectangle<int> _sourceRect = new(0, 0, 32, 32);
        private readonly int _textureId;
        private double _fallSpeed; 

        private double _timeOnGround = 0;
        
        private double _maxTimeOnGround; 

        public TreatObject(double startX, double startY, double fallSpeed, ItemType type, GameRenderer renderer) : base()
        {
            X = startX;
            Y = startY;
            _fallSpeed = fallSpeed;
            Type = type;

            // we load texture and set ground duration based on the item type
            if (Type == ItemType.Choco)
            {
                _textureId = renderer.LoadTexture(Path.Combine("Assets", "choco.png"), out _);
                _maxTimeOnGround = 8000.0; // choco stays more than normal treats
            }
            else
            {
                _textureId = renderer.LoadTexture(Path.Combine("Assets", "treat.png"), out _);
                _maxTimeOnGround = 5000.0; // normal treat duration
            }

            UpdateTarget();
        }

        public bool Update(double msSinceLastFrame, int groundLevel)
        {
            if (Y < groundLevel)
            {
                var pixelsToMove = _fallSpeed * (msSinceLastFrame / 1000.0);
                Y += pixelsToMove;
                if (Y >= groundLevel)
                {
                    Y = groundLevel;
                }
            }
            else
            {
                _timeOnGround += msSinceLastFrame;
            }

            UpdateTarget();

            return _timeOnGround <= _maxTimeOnGround;
        }

        private void UpdateTarget()
        {
            TextureDestination = new Rectangle<int>((int)X, (int)Y, 32, 32);
        }

        public void Render(GameRenderer renderer)
        {
            renderer.RenderTexture(_textureId, _sourceRect, TextureDestination);
        }
    }
}