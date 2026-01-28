using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AVAInvaders.Models;
using AVAInvaders.Services;

namespace AVAInvaders.Views;

public class GameCanvas : Control
{
    private readonly GameEngine _gameEngine;
    private readonly InputService _inputService;
    private readonly SoundService _soundService;
    private readonly DispatcherTimer _gameTimer;
    private DateTime _lastUpdate;

    // Colors
    private static readonly Color BackgroundColor = Color.FromRgb(0, 0, 0);
    private static readonly Color PlayerColor = Color.FromRgb(0, 255, 0);
    private static readonly Color InvaderSmallColor = Color.FromRgb(255, 0, 255);
    private static readonly Color InvaderMediumColor = Color.FromRgb(0, 255, 255);
    private static readonly Color InvaderLargeColor = Color.FromRgb(255, 255, 0);
    private static readonly Color BulletColor = Color.FromRgb(255, 255, 255);
    private static readonly Color BunkerColor = Color.FromRgb(0, 255, 0);
    private static readonly Color UfoColor = Color.FromRgb(255, 0, 0);
    private static readonly Color TextColor = Color.FromRgb(255, 255, 255);
    private static readonly Color ExplosionColor = Color.FromRgb(255, 165, 0);

    private readonly IBrush _backgroundBrush;
    private readonly IBrush _playerBrush;
    private readonly IBrush _bulletBrush;
    private readonly IBrush _bunkerBrush;
    private readonly IBrush _ufoBrush;
    private readonly IBrush _textBrush;
    private readonly IBrush _explosionBrush;

    private readonly Typeface _gameFont;

    public GameCanvas()
    {
        _soundService = new SoundService();
        _inputService = new InputService();
        _gameEngine = new GameEngine(_soundService);

        _backgroundBrush = new SolidColorBrush(BackgroundColor);
        _playerBrush = new SolidColorBrush(PlayerColor);
        _bulletBrush = new SolidColorBrush(BulletColor);
        _bunkerBrush = new SolidColorBrush(BunkerColor);
        _ufoBrush = new SolidColorBrush(UfoColor);
        _textBrush = new SolidColorBrush(TextColor);
        _explosionBrush = new SolidColorBrush(ExplosionColor);

        _gameFont = new Typeface("Courier New", FontStyle.Normal, FontWeight.Bold);

        Focusable = true;

        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _gameTimer.Tick += GameLoop;

        _soundService.Initialize();
        _inputService.Initialize();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lastUpdate = DateTime.Now;
        _gameTimer.Start();
        Focus();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _gameTimer.Stop();
        _soundService.Dispose();
        _inputService.Dispose();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _inputService.KeyDown(e.Key);
        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        _inputService.KeyUp(e.Key);
        e.Handled = true;
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        double deltaTime = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        deltaTime = Math.Min(deltaTime, 0.1);

        _inputService.Update();
        _gameEngine.Update(deltaTime, _inputService);

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double scaleX = Bounds.Width / GameEngine.GameWidth;
        double scaleY = Bounds.Height / GameEngine.GameHeight;
        double scale = Math.Min(scaleX, scaleY);

        double offsetX = (Bounds.Width - GameEngine.GameWidth * scale) / 2;
        double offsetY = (Bounds.Height - GameEngine.GameHeight * scale) / 2;

        // Draw background
        context.FillRectangle(_backgroundBrush, new Avalonia.Rect(0, 0, Bounds.Width, Bounds.Height));

        using (context.PushTransform(Matrix.CreateTranslation(offsetX, offsetY)))
        using (context.PushTransform(Matrix.CreateScale(scale, scale)))
        {
            DrawGame(context);
        }
    }

    private void DrawGame(DrawingContext context)
    {
        // Draw HUD
        DrawHud(context);

        switch (_gameEngine.State)
        {
            case GameState.Title:
                DrawTitleScreen(context);
                break;

            case GameState.Playing:
            case GameState.Paused:
                DrawGameplay(context);
                if (_gameEngine.State == GameState.Paused)
                    DrawPauseOverlay(context);
                break;

            case GameState.GameOver:
                DrawGameplay(context);
                DrawGameOverOverlay(context);
                break;

            case GameState.LevelComplete:
                DrawGameplay(context);
                DrawLevelCompleteOverlay(context);
                break;
        }
    }

    private void DrawHud(DrawingContext context)
    {
        var scoreText = new FormattedText(
            $"SCORE: {_gameEngine.Score:D5}",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            _textBrush);
        context.DrawText(scoreText, new Point(20, 20));

        var hiScoreText = new FormattedText(
            $"HI-SCORE: {_gameEngine.HighScore:D5}",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            _textBrush);
        context.DrawText(hiScoreText, new Point(GameEngine.GameWidth / 2 - 100, 20));

        var livesText = new FormattedText(
            $"LIVES: {_gameEngine.Player.Lives}",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            _textBrush);
        context.DrawText(livesText, new Point(GameEngine.GameWidth - 150, 20));

        var levelText = new FormattedText(
            $"LEVEL: {_gameEngine.Level}",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            _textBrush);
        context.DrawText(levelText, new Point(GameEngine.GameWidth - 150, 50));
    }

    private void DrawTitleScreen(DrawingContext context)
    {
        var titleText = new FormattedText(
            "AVA INVADERS",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            64,
            new SolidColorBrush(PlayerColor));
        context.DrawText(titleText, new Point(GameEngine.GameWidth / 2 - titleText.Width / 2, 200));

        var subtitleText = new FormattedText(
            "A Space Invaders Clone",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            _textBrush);
        context.DrawText(subtitleText, new Point(GameEngine.GameWidth / 2 - subtitleText.Width / 2, 280));

        // Draw sample invaders
        DrawInvaderSprite(context, InvaderType.Small, GameEngine.GameWidth / 2 - 120, 350, 0);
        var smallText = new FormattedText("= 30 PTS", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _gameFont, 20, _textBrush);
        context.DrawText(smallText, new Point(GameEngine.GameWidth / 2 - 60, 358));

        DrawInvaderSprite(context, InvaderType.Medium, GameEngine.GameWidth / 2 - 120, 400, 0);
        var medText = new FormattedText("= 20 PTS", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _gameFont, 20, _textBrush);
        context.DrawText(medText, new Point(GameEngine.GameWidth / 2 - 60, 408));

        DrawInvaderSprite(context, InvaderType.Large, GameEngine.GameWidth / 2 - 120, 450, 0);
        var largeText = new FormattedText("= 10 PTS", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _gameFont, 20, _textBrush);
        context.DrawText(largeText, new Point(GameEngine.GameWidth / 2 - 60, 458));

        DrawUfoSprite(context, GameEngine.GameWidth / 2 - 120, 500);
        var ufoText = new FormattedText("= ??? PTS", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _gameFont, 20, _textBrush);
        context.DrawText(ufoText, new Point(GameEngine.GameWidth / 2 - 60, 508));

        var startText = new FormattedText(
            "PRESS SPACE OR START TO PLAY",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            28,
            new SolidColorBrush(Color.FromRgb(255, 255, 0)));
        context.DrawText(startText, new Point(GameEngine.GameWidth / 2 - startText.Width / 2, 580));

        var controlsText = new FormattedText(
            "CONTROLS: ARROWS/WASD = MOVE   SPACE = FIRE   P/ESC = PAUSE",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            16,
            _textBrush);
        context.DrawText(controlsText, new Point(GameEngine.GameWidth / 2 - controlsText.Width / 2, 650));

        var gamepadText = new FormattedText(
            "GAMEPAD: D-PAD/STICK = MOVE   A/RT = FIRE   START = BEGIN   BACK = PAUSE",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            16,
            _textBrush);
        context.DrawText(gamepadText, new Point(GameEngine.GameWidth / 2 - gamepadText.Width / 2, 680));
    }

    private void DrawGameplay(DrawingContext context)
    {
        // Draw bunkers
        foreach (var bunker in _gameEngine.Bunkers)
        {
            DrawBunker(context, bunker);
        }

        // Draw player
        if (!_gameEngine.Player.IsExploding)
        {
            DrawPlayerSprite(context, _gameEngine.Player);
        }
        else
        {
            DrawExplosion(context, _gameEngine.Player.X, _gameEngine.Player.Y, _gameEngine.Player.Width, _gameEngine.Player.Height);
        }

        // Draw invaders
        foreach (var invader in _gameEngine.Invaders.Where(i => i.IsAlive))
        {
            DrawInvaderSprite(context, invader.Type, invader.X, invader.Y, invader.AnimationFrame);
        }

        // Draw UFO
        if (_gameEngine.Ufo != null)
        {
            DrawUfoSprite(context, _gameEngine.Ufo.X, _gameEngine.Ufo.Y);
        }

        // Draw bullets
        foreach (var bullet in _gameEngine.Bullets)
        {
            if (bullet.IsPlayerBullet)
            {
                context.FillRectangle(_bulletBrush, new Avalonia.Rect(bullet.X, bullet.Y, bullet.Width, bullet.Height));
            }
            else
            {
                DrawInvaderBullet(context, bullet);
            }
        }
    }

    private void DrawPlayerSprite(DrawingContext context, Player player)
    {
        // Classic player ship shape
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(player.X + player.Width / 2, player.Y), IsClosed = true };

        figure.Segments!.Add(new LineSegment { Point = new Point(player.X + player.Width, player.Y + player.Height) });
        figure.Segments.Add(new LineSegment { Point = new Point(player.X + player.Width - 8, player.Y + player.Height) });
        figure.Segments.Add(new LineSegment { Point = new Point(player.X + player.Width - 8, player.Y + player.Height - 8) });
        figure.Segments.Add(new LineSegment { Point = new Point(player.X + 8, player.Y + player.Height - 8) });
        figure.Segments.Add(new LineSegment { Point = new Point(player.X + 8, player.Y + player.Height) });
        figure.Segments.Add(new LineSegment { Point = new Point(player.X, player.Y + player.Height) });

        geometry.Figures!.Add(figure);
        context.DrawGeometry(_playerBrush, null, geometry);

        // Cannon
        context.FillRectangle(_playerBrush, new Avalonia.Rect(
            player.X + player.Width / 2 - 3,
            player.Y - 8,
            6, 12));
    }

    private void DrawInvaderSprite(DrawingContext context, InvaderType type, double x, double y, int frame)
    {
        var brush = type switch
        {
            InvaderType.Small => new SolidColorBrush(InvaderSmallColor),
            InvaderType.Medium => new SolidColorBrush(InvaderMediumColor),
            _ => new SolidColorBrush(InvaderLargeColor)
        };

        int[,] sprite = GetInvaderSprite(type, frame);
        const int pixelSize = 4;

        for (int py = 0; py < sprite.GetLength(0); py++)
        {
            for (int px = 0; px < sprite.GetLength(1); px++)
            {
                if (sprite[py, px] == 1)
                {
                    context.FillRectangle(brush, new Avalonia.Rect(
                        x + px * pixelSize,
                        y + py * pixelSize,
                        pixelSize, pixelSize));
                }
            }
        }
    }

    private int[,] GetInvaderSprite(InvaderType type, int frame)
    {
        return type switch
        {
            InvaderType.Small => frame == 0 ? SmallInvaderFrame1 : SmallInvaderFrame2,
            InvaderType.Medium => frame == 0 ? MediumInvaderFrame1 : MediumInvaderFrame2,
            _ => frame == 0 ? LargeInvaderFrame1 : LargeInvaderFrame2
        };
    }

    // Small invader (top row) sprites
    private static readonly int[,] SmallInvaderFrame1 =
    {
        {0,0,0,0,1,1,0,0,0,0,0},
        {0,0,0,1,1,1,1,0,0,0,0},
        {0,0,1,1,1,1,1,1,0,0,0},
        {0,1,1,0,1,1,0,1,1,0,0},
        {0,1,1,1,1,1,1,1,1,0,0},
        {0,0,0,1,0,0,1,0,0,0,0},
        {0,0,1,0,1,1,0,1,0,0,0},
        {0,1,0,1,0,0,1,0,1,0,0}
    };

    private static readonly int[,] SmallInvaderFrame2 =
    {
        {0,0,0,0,1,1,0,0,0,0,0},
        {0,0,0,1,1,1,1,0,0,0,0},
        {0,0,1,1,1,1,1,1,0,0,0},
        {0,1,1,0,1,1,0,1,1,0,0},
        {0,1,1,1,1,1,1,1,1,0,0},
        {0,0,1,0,0,0,0,1,0,0,0},
        {0,1,0,0,1,1,0,0,1,0,0},
        {0,0,1,0,0,0,0,1,0,0,0}
    };

    // Medium invader sprites
    private static readonly int[,] MediumInvaderFrame1 =
    {
        {0,0,1,0,0,0,0,0,1,0,0},
        {0,0,0,1,0,0,0,1,0,0,0},
        {0,0,1,1,1,1,1,1,1,0,0},
        {0,1,1,0,1,1,1,0,1,1,0},
        {1,1,1,1,1,1,1,1,1,1,1},
        {1,0,1,1,1,1,1,1,1,0,1},
        {1,0,1,0,0,0,0,0,1,0,1},
        {0,0,0,1,1,0,1,1,0,0,0}
    };

    private static readonly int[,] MediumInvaderFrame2 =
    {
        {0,0,1,0,0,0,0,0,1,0,0},
        {1,0,0,1,0,0,0,1,0,0,1},
        {1,0,1,1,1,1,1,1,1,0,1},
        {1,1,1,0,1,1,1,0,1,1,1},
        {1,1,1,1,1,1,1,1,1,1,1},
        {0,1,1,1,1,1,1,1,1,1,0},
        {0,0,1,0,0,0,0,0,1,0,0},
        {0,1,0,0,0,0,0,0,0,1,0}
    };

    // Large invader sprites
    private static readonly int[,] LargeInvaderFrame1 =
    {
        {0,0,0,0,1,1,1,1,0,0,0,0},
        {0,1,1,1,1,1,1,1,1,1,1,0},
        {1,1,1,1,1,1,1,1,1,1,1,1},
        {1,1,1,0,0,1,1,0,0,1,1,1},
        {1,1,1,1,1,1,1,1,1,1,1,1},
        {0,0,0,1,1,0,0,1,1,0,0,0},
        {0,0,1,1,0,1,1,0,1,1,0,0},
        {1,1,0,0,0,0,0,0,0,0,1,1}
    };

    private static readonly int[,] LargeInvaderFrame2 =
    {
        {0,0,0,0,1,1,1,1,0,0,0,0},
        {0,1,1,1,1,1,1,1,1,1,1,0},
        {1,1,1,1,1,1,1,1,1,1,1,1},
        {1,1,1,0,0,1,1,0,0,1,1,1},
        {1,1,1,1,1,1,1,1,1,1,1,1},
        {0,0,1,1,1,0,0,1,1,1,0,0},
        {0,1,1,0,0,1,1,0,0,1,1,0},
        {0,0,1,1,0,0,0,0,1,1,0,0}
    };

    private void DrawUfoSprite(DrawingContext context, double x, double y)
    {
        var ellipseGeometry = new EllipseGeometry(new Avalonia.Rect(x + 8, y + 8, 48, 16));
        context.DrawGeometry(_ufoBrush, null, ellipseGeometry);

        var domeGeometry = new EllipseGeometry(new Avalonia.Rect(x + 20, y, 24, 12));
        context.DrawGeometry(_ufoBrush, null, domeGeometry);

        // Lights
        var lightBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
        context.FillRectangle(lightBrush, new Avalonia.Rect(x + 16, y + 20, 4, 4));
        context.FillRectangle(lightBrush, new Avalonia.Rect(x + 30, y + 20, 4, 4));
        context.FillRectangle(lightBrush, new Avalonia.Rect(x + 44, y + 20, 4, 4));
    }

    private void DrawBunker(DrawingContext context, Bunker bunker)
    {
        for (int x = 0; x < Bunker.PixelWidth; x++)
        {
            for (int y = 0; y < Bunker.PixelHeight; y++)
            {
                if (bunker.Pixels[x, y])
                {
                    context.FillRectangle(_bunkerBrush, new Avalonia.Rect(
                        bunker.X + x,
                        bunker.Y + y,
                        1, 1));
                }
            }
        }
    }

    private void DrawInvaderBullet(DrawingContext context, Bullet bullet)
    {
        // Zigzag pattern for invader bullets
        var brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        int segments = 4;
        double segmentHeight = bullet.Height / segments;

        for (int i = 0; i < segments; i++)
        {
            double offsetX = (i % 2 == 0) ? 0 : 2;
            context.FillRectangle(brush, new Avalonia.Rect(
                bullet.X + offsetX,
                bullet.Y + i * segmentHeight,
                2, segmentHeight));
        }
    }

    private void DrawExplosion(DrawingContext context, double x, double y, double width, double height)
    {
        var random = new Random();
        for (int i = 0; i < 20; i++)
        {
            double px = x + random.NextDouble() * width;
            double py = y + random.NextDouble() * height;
            double size = random.NextDouble() * 6 + 2;
            context.FillRectangle(_explosionBrush, new Avalonia.Rect(px, py, size, size));
        }
    }

    private void DrawPauseOverlay(DrawingContext context)
    {
        var overlayBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
        context.FillRectangle(overlayBrush, new Avalonia.Rect(0, 0, GameEngine.GameWidth, GameEngine.GameHeight));

        var pauseText = new FormattedText(
            "PAUSED",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            64,
            _textBrush);
        context.DrawText(pauseText, new Point(GameEngine.GameWidth / 2 - pauseText.Width / 2, GameEngine.GameHeight / 2 - 50));

        var continueText = new FormattedText(
            "PRESS P, ESC, OR START TO CONTINUE",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            _textBrush);
        context.DrawText(continueText, new Point(GameEngine.GameWidth / 2 - continueText.Width / 2, GameEngine.GameHeight / 2 + 30));
    }

    private void DrawGameOverOverlay(DrawingContext context)
    {
        var overlayBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
        context.FillRectangle(overlayBrush, new Avalonia.Rect(0, 0, GameEngine.GameWidth, GameEngine.GameHeight));

        var gameOverText = new FormattedText(
            "GAME OVER",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            64,
            new SolidColorBrush(Color.FromRgb(255, 0, 0)));
        context.DrawText(gameOverText, new Point(GameEngine.GameWidth / 2 - gameOverText.Width / 2, GameEngine.GameHeight / 2 - 80));

        var scoreText = new FormattedText(
            $"FINAL SCORE: {_gameEngine.Score}",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            32,
            _textBrush);
        context.DrawText(scoreText, new Point(GameEngine.GameWidth / 2 - scoreText.Width / 2, GameEngine.GameHeight / 2));

        var restartText = new FormattedText(
            "PRESS SPACE OR START TO PLAY AGAIN",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            24,
            new SolidColorBrush(Color.FromRgb(255, 255, 0)));
        context.DrawText(restartText, new Point(GameEngine.GameWidth / 2 - restartText.Width / 2, GameEngine.GameHeight / 2 + 60));
    }

    private void DrawLevelCompleteOverlay(DrawingContext context)
    {
        var overlayBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        context.FillRectangle(overlayBrush, new Avalonia.Rect(0, 0, GameEngine.GameWidth, GameEngine.GameHeight));

        var levelText = new FormattedText(
            $"LEVEL {_gameEngine.Level} COMPLETE!",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _gameFont,
            48,
            new SolidColorBrush(Color.FromRgb(0, 255, 0)));
        context.DrawText(levelText, new Point(GameEngine.GameWidth / 2 - levelText.Width / 2, GameEngine.GameHeight / 2 - 30));
    }
}
