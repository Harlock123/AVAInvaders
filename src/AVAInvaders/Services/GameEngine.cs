using AVAInvaders.Models;

namespace AVAInvaders.Services;

public enum GameState
{
    Title,
    Playing,
    Paused,
    GameOver,
    LevelComplete
}

public class GameEngine
{
    public const double GameWidth = 672;
    public const double GameHeight = 768;
    public const int InvaderColumns = 11;
    public const int InvaderRows = 5;

    private const double InvaderStartY = 120;
    private const double InvaderSpacingX = 52;
    private const double InvaderSpacingY = 44;
    private const double PlayerY = 680;
    private const double BunkerY = 580;

    private readonly SoundService _soundService;
    private readonly Random _random = new();

    private double _invaderDirection = 1;
    private double _invaderSpeed = 1.0;
    private double _invaderMoveTimer;
    private double _invaderShootTimer;
    private double _ufoTimer;
    private double _levelCompleteTimer;
    private double _gameTime;
    private int _marchSoundIndex;

    public Player Player { get; private set; } = new();
    public List<Invader> Invaders { get; } = new();
    public List<Bullet> Bullets { get; } = new();
    public List<Bunker> Bunkers { get; } = new();
    public Ufo? Ufo { get; private set; }

    public int Score { get; private set; }
    public int HighScore { get; set; }
    public int Level { get; private set; } = 1;
    public GameState State { get; private set; } = GameState.Title;

    public event Action? StateChanged;
    public event Action? ScoreChanged;

    public GameEngine(SoundService soundService)
    {
        _soundService = soundService;
    }

    public void StartGame()
    {
        Score = 0;
        Level = 1;
        _gameTime = 0;
        Player = new Player
        {
            X = GameWidth / 2 - 26,
            Y = PlayerY,
            Lives = 3
        };

        InitializeLevel();
        State = GameState.Playing;
        StateChanged?.Invoke();
        ScoreChanged?.Invoke();
    }

    private void InitializeLevel()
    {
        Invaders.Clear();
        Bullets.Clear();
        Ufo = null;

        _invaderDirection = 1;
        _invaderSpeed = 1.0 + (Level - 1) * 0.15;
        _invaderMoveTimer = 0;
        _invaderShootTimer = 0;
        _ufoTimer = _random.NextDouble() * 20 + 10;

        // Create invaders
        double startX = (GameWidth - InvaderColumns * InvaderSpacingX) / 2;

        for (int row = 0; row < InvaderRows; row++)
        {
            var type = row switch
            {
                0 => InvaderType.Small,
                1 or 2 => InvaderType.Medium,
                _ => InvaderType.Large
            };

            for (int col = 0; col < InvaderColumns; col++)
            {
                Invaders.Add(new Invader(type)
                {
                    X = startX + col * InvaderSpacingX,
                    Y = InvaderStartY + row * InvaderSpacingY
                });
            }
        }

        // Create bunkers (only on level 1 or when reset)
        if (Bunkers.Count == 0)
        {
            double bunkerSpacing = GameWidth / 5;
            for (int i = 0; i < 4; i++)
            {
                Bunkers.Add(new Bunker
                {
                    X = bunkerSpacing * (i + 0.5) - Bunker.PixelWidth / 2,
                    Y = BunkerY
                });
            }
        }
    }

    public void Update(double deltaTime, InputService input)
    {
        switch (State)
        {
            case GameState.Title:
                if (input.Start || input.Fire)
                {
                    StartGame();
                }
                break;

            case GameState.Playing:
                if (input.Pause)
                {
                    State = GameState.Paused;
                    StateChanged?.Invoke();
                    return;
                }
                UpdatePlaying(deltaTime, input);
                break;

            case GameState.Paused:
                if (input.Pause || input.Start)
                {
                    State = GameState.Playing;
                    StateChanged?.Invoke();
                }
                break;

            case GameState.GameOver:
                if (input.Start || input.Fire)
                {
                    StartGame();
                }
                break;

            case GameState.LevelComplete:
                _levelCompleteTimer -= deltaTime;
                if (_levelCompleteTimer <= 0)
                {
                    Level++;
                    InitializeLevel();
                    State = GameState.Playing;
                    StateChanged?.Invoke();
                }
                break;
        }
    }

    private void UpdatePlaying(double deltaTime, InputService input)
    {
        _gameTime += deltaTime;
        UpdatePlayer(deltaTime, input);
        UpdateInvaders(deltaTime);
        UpdateBullets(deltaTime);
        UpdateUfo(deltaTime);
        CheckCollisions();
        CheckWinCondition();
    }

    private void UpdatePlayer(double deltaTime, InputService input)
    {
        if (Player.IsExploding)
        {
            Player.ExplosionTime -= deltaTime;
            if (Player.ExplosionTime <= 0)
            {
                Player.IsExploding = false;
                if (Player.Lives <= 0)
                {
                    State = GameState.GameOver;
                    if (Score > HighScore)
                        HighScore = Score;
                    StateChanged?.Invoke();
                }
                else
                {
                    Player.X = GameWidth / 2 - 26;
                }
            }
            return;
        }

        if (input.MoveLeft)
        {
            Player.X = Math.Max(0, Player.X - Player.Speed * deltaTime);
        }
        if (input.MoveRight)
        {
            Player.X = Math.Min(GameWidth - Player.Width, Player.X + Player.Speed * deltaTime);
        }

        if (input.Fire && Player.CanShoot(_gameTime))
        {
            Bullets.Add(new Bullet(true)
            {
                X = Player.X + Player.Width / 2 - 2,
                Y = Player.Y - 14
            });
            Player.LastShotTime = _gameTime;
            _soundService.PlaySound("shoot");
        }
    }

    private void UpdateInvaders(double deltaTime)
    {
        _invaderMoveTimer += deltaTime;

        int aliveCount = Invaders.Count(i => i.IsAlive);
        // Speed up as invaders are killed - starts slow, gets faster
        double speedMultiplier = 1.0 + (55 - aliveCount) * 0.05;
        // Base interval of 0.6 seconds at start, gets faster as invaders die
        double moveInterval = 0.6 / (_invaderSpeed * speedMultiplier);

        if (_invaderMoveTimer >= moveInterval)
        {
            _invaderMoveTimer = 0;

            bool shouldDescend = false;

            foreach (var invader in Invaders.Where(i => i.IsAlive))
            {
                invader.X += 8 * _invaderDirection;
                invader.AnimationFrame = (invader.AnimationFrame + 1) % 2;

                if (invader.X <= 0 || invader.X + invader.Width >= GameWidth)
                {
                    shouldDescend = true;
                }
            }

            if (shouldDescend)
            {
                _invaderDirection *= -1;
                foreach (var invader in Invaders.Where(i => i.IsAlive))
                {
                    invader.Y += 16;

                    if (invader.Y + invader.Height >= Player.Y)
                    {
                        State = GameState.GameOver;
                        if (Score > HighScore)
                            HighScore = Score;
                        StateChanged?.Invoke();
                    }
                }
            }

            _soundService.PlaySound($"march{_marchSoundIndex + 1}");
            _marchSoundIndex = (_marchSoundIndex + 1) % 4;
        }

        // Invader shooting
        _invaderShootTimer += deltaTime;
        double shootInterval = Math.Max(0.5, 2.0 - Level * 0.1);

        if (_invaderShootTimer >= shootInterval)
        {
            _invaderShootTimer = 0;

            var aliveInvaders = Invaders.Where(i => i.IsAlive).ToList();
            if (aliveInvaders.Count > 0)
            {
                var shooter = aliveInvaders[_random.Next(aliveInvaders.Count)];
                Bullets.Add(new Bullet(false)
                {
                    X = shooter.X + shooter.Width / 2 - 2,
                    Y = shooter.Y + shooter.Height
                });
            }
        }
    }

    private void UpdateBullets(double deltaTime)
    {
        foreach (var bullet in Bullets.Where(b => b.IsAlive))
        {
            if (bullet.IsPlayerBullet)
            {
                bullet.Y -= bullet.Speed * deltaTime;
                if (bullet.Y + bullet.Height < 0)
                    bullet.IsAlive = false;
            }
            else
            {
                bullet.Y += bullet.Speed * deltaTime;
                if (bullet.Y > GameHeight)
                    bullet.IsAlive = false;
            }
        }

        Bullets.RemoveAll(b => !b.IsAlive);
    }

    private void UpdateUfo(double deltaTime)
    {
        if (Ufo == null)
        {
            _ufoTimer -= deltaTime;
            if (_ufoTimer <= 0)
            {
                bool movingRight = _random.Next(2) == 0;
                Ufo = new Ufo(movingRight)
                {
                    X = movingRight ? -64 : GameWidth,
                    Y = 60
                };
                _ufoTimer = _random.NextDouble() * 20 + 15;
                _soundService.PlaySound("ufo");
            }
        }
        else
        {
            Ufo.X += (Ufo.MovingRight ? 1 : -1) * Ufo.Speed * deltaTime;

            if ((Ufo.MovingRight && Ufo.X > GameWidth) || (!Ufo.MovingRight && Ufo.X + Ufo.Width < 0))
            {
                Ufo = null;
            }
        }
    }

    private void CheckCollisions()
    {
        // Player bullets vs invaders
        foreach (var bullet in Bullets.Where(b => b.IsAlive && b.IsPlayerBullet))
        {
            foreach (var invader in Invaders.Where(i => i.IsAlive))
            {
                if (bullet.CollidesWith(invader))
                {
                    bullet.IsAlive = false;
                    invader.IsAlive = false;
                    Score += invader.Points;
                    ScoreChanged?.Invoke();
                    _soundService.PlaySound("invaderKilled");
                    break;
                }
            }

            // Player bullet vs UFO
            if (Ufo != null && bullet.CollidesWith(Ufo))
            {
                bullet.IsAlive = false;
                Score += Ufo.Points;
                ScoreChanged?.Invoke();
                _soundService.PlaySound("explosion");
                Ufo = null;
            }
        }

        // Invader bullets vs player
        if (!Player.IsExploding)
        {
            foreach (var bullet in Bullets.Where(b => b.IsAlive && !b.IsPlayerBullet))
            {
                if (bullet.CollidesWith(Player))
                {
                    bullet.IsAlive = false;
                    PlayerHit();
                    break;
                }
            }
        }

        // Invaders vs player
        foreach (var invader in Invaders.Where(i => i.IsAlive))
        {
            if (!Player.IsExploding && invader.CollidesWith(Player))
            {
                PlayerHit();
                break;
            }
        }

        // Bullets vs bunkers
        foreach (var bunker in Bunkers)
        {
            foreach (var bullet in Bullets.Where(b => b.IsAlive))
            {
                if (bunker.CheckPixelCollision(bullet.X, bullet.Y, bullet.Width, bullet.Height))
                {
                    bunker.TakeDamage(bullet.X + bullet.Width / 2,
                        bullet.IsPlayerBullet ? bullet.Y : bullet.Y + bullet.Height,
                        bullet.IsPlayerBullet ? 3 : 5);
                    bullet.IsAlive = false;
                }
            }
        }

        // Invaders vs bunkers
        foreach (var bunker in Bunkers)
        {
            foreach (var invader in Invaders.Where(i => i.IsAlive))
            {
                if (bunker.Bounds.X < invader.X + invader.Width &&
                    bunker.Bounds.X + bunker.Width > invader.X &&
                    bunker.Bounds.Y < invader.Y + invader.Height &&
                    bunker.Bounds.Y + bunker.Height > invader.Y)
                {
                    // Invader destroys overlapping bunker section
                    for (int x = 0; x < Bunker.PixelWidth; x++)
                    {
                        for (int y = 0; y < Bunker.PixelHeight; y++)
                        {
                            double worldX = bunker.X + x;
                            double worldY = bunker.Y + y;
                            if (worldX >= invader.X && worldX < invader.X + invader.Width &&
                                worldY >= invader.Y && worldY < invader.Y + invader.Height)
                            {
                                bunker.Pixels[x, y] = false;
                            }
                        }
                    }
                }
            }
        }
    }

    private void PlayerHit()
    {
        Player.Lives--;
        Player.IsExploding = true;
        Player.ExplosionTime = 1.5;
        _soundService.PlaySound("playerDeath");
    }

    private void CheckWinCondition()
    {
        if (Invaders.All(i => !i.IsAlive))
        {
            State = GameState.LevelComplete;
            _levelCompleteTimer = 2.0;
            StateChanged?.Invoke();
        }
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
        {
            State = GameState.Paused;
            StateChanged?.Invoke();
        }
        else if (State == GameState.Paused)
        {
            State = GameState.Playing;
            StateChanged?.Invoke();
        }
    }
}
