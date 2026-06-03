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

        private int _highScore = 0;
        private const string HighScoreFile = "highscore.txt";
        private int _lives = 3;
        public bool _isGameOver = false;
        private int _heartTextureId;
        private Rectangle<int> _heartSource;

        private int _level = 1;
        private double _currentFallSpeed = 150.0;
        private double _currentSpawnInterval = 1500;
        private int _nextLevelScore = 50;

        public GameLogic(GameRenderer renderer)
        {
            _renderer = renderer;
        }

        // load BG, start & loop BG music
        public void InitializeGame()
        {
            if (File.Exists(HighScoreFile))
            {
                string scoreText = File.ReadAllText(HighScoreFile);
                if (int.TryParse(scoreText, out int savedScore))
                {
                    _highScore = savedScore;
                }
            }
            Console.WriteLine($"Current High Score to beat is: {_highScore}");
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
                if (_spawnTimer > _currentSpawnInterval) 
                {
                    // we attempt to avoid overlap between falling objects
                    int randomX = 0;
                    bool validPosition = false;
                    int attempts = 0;
                    
                    // try up to 10 times to find a spot that is at least 40 pixels away from other items
                    while (!validPosition && attempts < 10)
                    {
                        randomX = _random.Next(0, 1000 - 32);
                        validPosition = true;
                        
                        foreach (var existingItem in _treats.Values)
                        {
                            if (Math.Abs(existingItem.X - randomX) < 40)
                            {
                                validPosition = false;
                                break;
                            }
                        }
                        attempts++;
                    }

                    bool spawnChoco = _random.NextDouble() < 0.25; 
                    ItemType newItemType = spawnChoco ? ItemType.Choco : ItemType.Treat;
                    
                    var newItem = new TreatObject(randomX, -32, _currentFallSpeed, newItemType, _renderer);
                    _treats.Add(newItem.Id, newItem);
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
                            isAlive = false;
                            wasCaught = true;

                            if (treat.Type == ItemType.Treat)
                            {
                                _score += 10;
                                Console.WriteLine($"Caught a treat! Score: {_score}");

                                if (_score >= _nextLevelScore)
                                {
                                    _level++;
                                    _nextLevelScore += 50;
                                    
                                    _currentFallSpeed += 40.0; // treats fall faster as levels increase
                                    _currentSpawnInterval = Math.Max(500.0, _currentSpawnInterval - 150.0); 

                                    Console.WriteLine($"\n*** LEVEL UP! Welcome to Level {_level} ***");
                                    Console.WriteLine($"New Speed: {_currentFallSpeed} | New Spawn Rate: {_currentSpawnInterval}ms\n");
                                }
                            }
                            else if (treat.Type == ItemType.Choco)
                            {
                                _lives--;
                                Console.WriteLine($"Yuck! Ate chocolate! Hearts remaining: {_lives}");
                                
                                if (_lives <= 0)
                                {
                                    TriggerGameOver();
                                }
                            }
                        }
                    }
                    
                    if (!isAlive) 
                    {
                        treatsToRemove.Add(kvp.Key);
                        
                        if (!wasCaught)
                        {
                            if (treat.Type == ItemType.Treat)
                            {
                                _lives--;
                                Console.WriteLine($"Missed a treat! Hearts remaining: {_lives}");
                                
                                if (_lives <= 0)
                                {
                                    TriggerGameOver();
                                }
                            }
                            else if (treat.Type == ItemType.Choco)
                            {
                                Console.WriteLine("Woo! Dodged chocolate.");
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

        private void TriggerGameOver()
        {
            _isGameOver = true;
            Console.WriteLine("\n==============================");
            Console.WriteLine("GAME OVER! Final Score: " + _score);
            
            if (_score > _highScore)
            {
                Console.WriteLine($"Woof! NEW HIGH SCORE! {_score} beats the old record of {_highScore}! Noice!");
                File.WriteAllText(HighScoreFile, _score.ToString());
            }
            else
            {
                Console.WriteLine($"You didn't beat the high score of {_highScore}. Better luck next time!");
            }
            Console.WriteLine("==============================\n");

            _audioPlayer.Stop();
        }
    }
}