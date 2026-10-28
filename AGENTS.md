# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers playing the game, its controls and its
features; this file covers how the code is put together and the rules that keep it working.

## What this is

A Space Invaders clone in C# on .NET 9 (`net9.0`) and Avalonia 11.3, drawn with Avalonia's `DrawingContext`. It is
one of the author's older AVA games: a single project with no tests. Gamepads come through SDL2 (`ppy.SDL2-CS`),
and the sound effects are generated in code at start-up. It runs on Windows, macOS and Linux, x64 and ARM.

## Commands

```bash
dotnet build AVAInvaders.sln                 # no global.json: any .NET 9 SDK
dotnet run --project src/AVAInvaders         # play it; there are no command-line options
```

There are no tests yet.

## Layout

- `src/AVAInvaders/Services/GameEngine.cs`: the game. It runs the states (`Title`, `Playing`, `Paused`,
  `LevelComplete`, `GameOver`), moves the player, the 5x11 invader formation, bullets and the UFO, and checks
  hits. The playfield is a fixed 672x768 (`GameWidth`, `GameHeight`).
- `src/AVAInvaders/Services/InputService.cs`: keys and the first SDL2 game controller, folded into the flags
  `MoveLeft`, `MoveRight`, `Fire`, `Pause` and `Start`.
- `src/AVAInvaders/Services/SoundService.cs`: builds WAV buffers for each effect and the four march tones.
  - On Windows it plays them with NAudio.
  - On Linux it writes a temporary WAV and runs `aplay`; on macOS it runs `afplay`.
- `src/AVAInvaders/Views/GameCanvas.cs`: the control that owns the services, runs the timer, passes keys to
  `InputService`, and draws everything scaled to fit the window.
- `src/AVAInvaders/Models/`: `GameObject` and its subclasses `Player`, `Invader`, `Bullet`, `Ufo` and `Bunker`.
  `Bunker` keeps a pixel grid that bullets erode.
- `Images/`: the README's screenshot.

## Rules that keep it working

- **Variable time step.** `GameCanvas` runs a `DispatcherTimer` at about 60 Hz and passes the real elapsed time,
  capped at 0.1 s, to `GameEngine.Update`. Movement and timers scale by `deltaTime`, so express new speeds per
  second.
- **Randomness is not seeded.** `GameEngine` uses its own `new Random()`, and explosions in `GameCanvas` make a
  new `Random` each draw. Nothing replays deterministically.
- **Input is flags, not axes.** Only left and right exist; there is no up/down `dy`. Add new controls to both the
  keyboard and gamepad branches of `InputService`.
- **SDL2 is optional.** If the SDL2 library is missing, `InputService` prints install hints and the game runs on
  the keyboard alone. Keep that fallback.
- **Sound must not block.** `PlaySound` runs each effect on `Task.Run`, and all playback errors are swallowed so
  a missing `aplay` or `afplay` never stops the game.
- **The high score is per session.** It lives in `GameEngine.HighScore` and is not saved to disk.

## Making a change

- Gameplay belongs in `GameEngine`; `GameCanvas` should only read state and draw it.
- Run the game to check a change, since there are no tests.
- Update `README.md` (features, controls) to match, and refresh the screenshot in `Images/` if the look changes.

## CI and releases

`.github/workflows/build.yml` builds every push and pull request with .NET 9 (there are no tests to run). It then
publishes self-contained single-file builds for win-x64/x86/arm64, linux-x64/arm64/arm and osx-x64/arm64, and
keeps them as workflow artifacts. Only pushes to `master` replace the assets of the `latest` pre-release, and a
`v*` tag makes a proper release. After a release it asks the portfolio site to rebuild, if the
`SITE_REFRESH_TOKEN` secret is set.
