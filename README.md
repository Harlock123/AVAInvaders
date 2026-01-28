# AVA Invaders

A classic Space Invaders clone built with .NET 9 and Avalonia UI.

## Features

### Gameplay
- **Classic Space Invaders mechanics** - Defend Earth from waves of descending alien invaders
- **5 rows x 11 columns of invaders** - 55 enemies per wave with authentic movement patterns
- **3 invader types** with different point values:
  - Small (top row): 30 points
  - Medium (middle rows): 20 points
  - Large (bottom rows): 10 points
- **Mystery UFO** - Appears randomly for bonus points (50, 100, 150, or 300 points)
- **4 destructible bunkers** - Pixel-by-pixel destruction from both player and enemy fire
- **Progressive difficulty** - Each level increases invader speed
- **Lives system** - Start with 3 lives
- **High score tracking** - Persists during session

### Controls

#### Keyboard
- **Arrow Keys / WASD** - Move left/right
- **Space / W / Up Arrow** - Fire
- **P / Escape** - Pause game
- **Enter / Space** - Start game / Continue

#### Gamepad (SDL2)
- **D-Pad / Left Stick** - Move left/right
- **A Button / Right Trigger** - Fire
- **Start Button** - Start game
- **Back/Select Button** - Pause

### Audio
- **Procedurally generated sound effects**:
  - Player shooting
  - Invader destruction
  - Player death explosion
  - UFO flyby
  - Invader march rhythm (4 alternating tones)
- **Cross-platform audio support** via NAudio (Windows) or native tools (Linux/macOS)

### Visual Features
- **Authentic pixel-art sprites** for all invader types
- **Animated invaders** - Two-frame animation synced with movement
- **Classic green/cyan/magenta/yellow color scheme**
- **Smooth 60 FPS rendering**
- **Responsive window scaling** - Maintains aspect ratio at any size
- **Title screen** with point values and controls display
- **Pause overlay**
- **Game over screen** with final score
- **Level complete notification**

### Technical Features
- **Built with .NET 9** - Latest .NET runtime
- **Avalonia UI framework** - True cross-platform desktop support
- **SDL2 gamepad support** - Industry-standard controller compatibility
- **No external assets required** - All graphics and sounds generated programmatically
- **Clean architecture** - Separated Models, Services, and Views

## Requirements

- .NET 9.0 SDK
- For gamepad support: SDL2 library installed on your system
- For sound on Linux: `aplay` (usually pre-installed)
- For sound on macOS: `afplay` (pre-installed)

## Building

```bash
cd src/AVAInvaders
dotnet build
```

## Running

```bash
cd src/AVAInvaders
dotnet run
```

## Project Structure

```
AVAInvaders/
├── src/
│   └── AVAInvaders/
│       ├── Models/           # Game object classes
│       │   ├── GameObject.cs
│       │   ├── Player.cs
│       │   ├── Invader.cs
│       │   ├── Bullet.cs
│       │   ├── Bunker.cs
│       │   └── Ufo.cs
│       ├── Services/         # Game logic and system services
│       │   ├── GameEngine.cs
│       │   ├── InputService.cs
│       │   └── SoundService.cs
│       ├── Views/            # UI components
│       │   └── GameCanvas.cs
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── MainWindow.axaml
│       ├── MainWindow.axaml.cs
│       └── Program.cs
├── AVAInvaders.sln
├── .gitignore
├── LICENSE
└── README.md
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Original Space Invaders by Tomohiro Nishikado (Taito, 1978)
- Avalonia UI Framework team
- SDL2 for cross-platform gamepad support
