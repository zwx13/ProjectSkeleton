using System;
using System.IO;
using Silk.NET.SDL;
using Silk.NET.Maths;

namespace CatcherGame.Models
{
    public class TreatObject : GameObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Rectangle<int> TextureDestination { get; private set; }
        
        private Rectangle<int> _sourceRect = new(0, 0, 32, 32);
        private readonly int _textureId;
        private const double FallSpeed = 150.0;
        private double _timeOnGround = 0;
        private const double MaxTimeOnGround = 2000.0;

        public TreatObject(double startX, double startY, GameRenderer renderer) : base()
        {
            X = startX;
            Y = startY;
            _textureId = renderer.LoadTexture(Path.Combine("Assets", "treat.png"), out _);
            UpdateTarget();
        }

        public bool Update(double msSinceLastFrame, int groundLevel)
        {
            if (Y < groundLevel)
            {
                var pixelsToMove = FallSpeed * (msSinceLastFrame / 1000.0);
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

            return _timeOnGround <= MaxTimeOnGround;
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