using System;
using System.Collections.Generic;
using System.IO;
using Silk.NET.Maths;
using CatcherGame.Models;
using NetCoreAudio;

namespace CatcherGame
{
    public class GameLogic
    {
        private readonly GameRenderer _renderer;
        private PlayerObject? _player;
        private Dictionary<int, TreatObject> _treats = new();
        
        private DateTimeOffset _lastUpdate;
        private double _spawnTimer = 0;
        private Random _random = new Random();
        private int _score = 0;

        private int _bgTextureId;
        private Rectangle<int> _bgSource;
        private Rectangle<int> _bgDest;

        private Player _audioPlayer = new Player();

        private int _lives = 3;
        public bool _isGameOver = false;
        private int _heartTextureId;
        private Rectangle<int> _heartSource;

        public GameLogic(GameRenderer renderer)
        {
            _renderer = renderer;
        }

        // load BG, start & loop BG music
        public void InitializeGame()
        {
            _player = new PlayerObject(_renderer);
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, 1000, 400));
            _lastUpdate = DateTimeOffset.Now;

            _bgTextureId = _renderer.LoadTexture(Path.Combine("Assets", "background.png"), out var bgData);
            _bgSource = new Rectangle<int>(0, 0, bgData.Width, bgData.Height);
            _bgDest = new Rectangle<int>(0, 0, 1000, 400);

            _heartTextureId = _renderer.LoadTexture(Path.Combine("Assets", "heart.png"), out var heartData);
            _heartSource = new Rectangle<int>(0, 0, heartData.Width, heartData.Height);

            string musicPath = Path.Combine("Assets", "background_music.mp3");
            if (File.Exists(musicPath))
            {
                _audioPlayer.Play(musicPath);
                _audioPlayer.PlaybackFinished += (sender, e) => 
                {
                    if (!_isGameOver) 
                    {
                        _audioPlayer.Play(musicPath);
                    }
                };
            }
        }
        
        public void UpdatePlayerPosition(double left, double right, double msSinceLastUpdate)
        {
            _player?.UpdatePosition(left, right, msSinceLastUpdate, 1000);
        }

        public void PlayerJump()
        {
            _player?.Jump();
        }

        public void PlayerDash(double direction)
        {
            _player?.Dash(direction);
        }

        public void RenderFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var msSinceLastFrame = (currentTime - _lastUpdate).TotalMilliseconds;
            _lastUpdate = currentTime;

            if (!_isGameOver)
            {
                _spawnTimer += msSinceLastFrame;
                if (_spawnTimer > 1500) 
                {
                    var randomX = _random.Next(0, 1000 - 32);
                    var newTreat = new TreatObject(randomX, -32, _renderer);
                    _treats.Add(newTreat.Id, newTreat);
                    _spawnTimer = 0;
                }

                List<int> treatsToRemove = new List<int>();
                foreach (var kvp in _treats)
                {
                    var treat = kvp.Value;
                    bool isAlive = treat.Update(msSinceLastFrame, 350);
                    bool wasCaught = false;

                    if (isAlive && _player != null)
                    {
                        if (CheckCollision(_player.TextureDestination, treat.TextureDestination))
                        {
                            _score += 10;
                            Console.WriteLine($"Caught! Score: {_score}");
                            isAlive = false;
                            wasCaught = true;
                        }
                    }
                    
                    if (!isAlive) 
                    {
                        treatsToRemove.Add(kvp.Key);
                        
                        if (!wasCaught)
                        {
                            _lives--;
                            Console.WriteLine($"Missed one! Hearts remaining: {_lives}");
                            
                            if (_lives <= 0)
                            {
                                _isGameOver = true;
                                Console.WriteLine("GAME OVER! Final Score: " + _score);
                                _audioPlayer.Stop();
                            }
                        }
                    }
                }

                foreach (var id in treatsToRemove) _treats.Remove(id);
            }

            // render
            _renderer.SetDrawColor(0, 0, 0, 255); 
            _renderer.ClearScreen();
            
            if (_player != null) _renderer.CameraLookAt((int)_player.X, 200); 

            _renderer.RenderTexture(_bgTextureId, _bgSource, _bgDest);
            
            if (!_isGameOver)
            {
                foreach (var treat in _treats.Values) treat.Render(_renderer);
                _player?.Render(_renderer);
            }

            var cameraX = _renderer.ToWorldCoordinates(0, 0).X; 
            for (int i = 0; i < _lives; i++)
            {
                var heartDest = new Rectangle<int>(cameraX + 20 + (i * 40), 20, 32, 32);
                _renderer.RenderTexture(_heartTextureId, _heartSource, heartDest);
            }

            _renderer.PresentFrame();
        }

        private bool CheckCollision(Rectangle<int> a, Rectangle<int> b)
        {
            bool overlapX = a.Origin.X < b.Origin.X + b.Size.X && a.Origin.X + a.Size.X > b.Origin.X;
            bool overlapY = a.Origin.Y < b.Origin.Y + b.Size.Y && a.Origin.Y + a.Size.Y > b.Origin.Y;
            return overlapX && overlapY;
        }

        public void StopMusic()
        {
            _audioPlayer.Stop();
        }
    }
}