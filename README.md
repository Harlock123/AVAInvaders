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

## Download and run

Ready-to-run builds for Windows, macOS and Linux, on both Intel/AMD and ARM, are on the
[Releases page](https://github.com/Harlock123/AVAInvaders/releases). **Latest build** is rebuilt on
every push to `master`. Each download is one self-contained executable: no .NET install is
needed.

```bash
tar -xzf AVAInvaders-linux-arm64.tar.gz      # or linux-x64, linux-arm, osx-arm64, osx-x64
./AVAInvaders
```

On Windows, unzip `AVAInvaders-win-x64.zip` (or `-win-arm64`) and run `AVAInvaders.exe`. On macOS, if
Gatekeeper refuses to open it, run `xattr -d com.apple.quarantine AVAInvaders` once.

### Linux: sound and gamepads

- **Sound** needs `aplay`, from the `alsa-utils` package. Without it the game runs but is
  silent. Minimal installs (Arch, Omarchy, some Raspberry Pi images) don't include it:

  ```bash
  sudo apt install alsa-utils             # Debian / Ubuntu / Raspberry Pi OS
  sudo pacman -S --needed alsa-utils      # Arch / Omarchy
  ```

- **Gamepads** use SDL2. It is bundled for Intel/AMD Windows, Intel Macs and Intel/AMD Linux,
  but not for ARM: on ARM Linux install it as below; on Apple Silicon Macs and Windows on ARM
  the controller isn't supported yet. Without SDL2 the keyboard still works:

  ```bash
  sudo apt install libsdl2-dev            # Debian / Ubuntu / Raspberry Pi OS
  sudo pacman -S --needed sdl2-compat     # Arch / Omarchy
  ```

  On Debian-based systems it has to be `libsdl2-dev`: the game looks for `libSDL2.so`, which
  the runtime-only `libsdl2-2.0-0` package doesn't provide.

## Requirements

- .NET 9.0 SDK
- For gamepad support: SDL2 library installed on your system
- For sound on Linux: `aplay` from `alsa-utils` (not on minimal installs; see
  [Linux: sound and gamepads](#linux-sound-and-gamepads))
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
