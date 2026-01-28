using Avalonia.Input;
using static SDL2.SDL;

namespace AVAInvaders.Services;

public class InputService : IDisposable
{
    private readonly HashSet<Key> _pressedKeys = new();
    private bool _disposed;

    // Gamepad state
    private nint _gameController;
    private bool _sdlInitialized;

    // Input state
    public bool MoveLeft { get; private set; }
    public bool MoveRight { get; private set; }
    public bool Fire { get; private set; }
    public bool Pause { get; private set; }
    public bool Start { get; private set; }

    public void Initialize()
    {
        try
        {
            InitializeSDL();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gamepad initialization failed: {ex.Message}");
        }
    }

    private void InitializeSDL()
    {
        try
        {
            if (SDL_Init(SDL_INIT_GAMECONTROLLER | SDL_INIT_JOYSTICK) < 0)
            {
                Console.WriteLine($"SDL init failed: {SDL_GetError()}");
                return;
            }

            _sdlInitialized = true;

            // Open first available game controller
            int numJoysticks = SDL_NumJoysticks();
            for (int i = 0; i < numJoysticks; i++)
            {
                if (SDL_IsGameController(i) == SDL_bool.SDL_TRUE)
                {
                    _gameController = SDL_GameControllerOpen(i);
                    if (_gameController != nint.Zero)
                    {
                        Console.WriteLine($"Controller connected: {SDL_GameControllerName(_gameController)}");
                        break;
                    }
                }
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("SDL2 library not found. Gamepad support disabled.");
            Console.WriteLine("Install SDL2 for gamepad support:");
            Console.WriteLine("  - Ubuntu/Debian: sudo apt install libsdl2-2.0-0");
            Console.WriteLine("  - Fedora: sudo dnf install SDL2");
            Console.WriteLine("  - Arch: sudo pacman -S sdl2");
            Console.WriteLine("  - macOS: brew install sdl2");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SDL initialization error: {ex.Message}");
        }
    }

    public void KeyDown(Key key)
    {
        _pressedKeys.Add(key);
    }

    public void KeyUp(Key key)
    {
        _pressedKeys.Remove(key);
    }

    public void Update()
    {
        // Reset transient states
        Start = false;
        Pause = false;
        Fire = false;
        MoveLeft = false;
        MoveRight = false;

        // Process keyboard input
        ProcessKeyboardInput();

        // Process gamepad input
        ProcessGamepadInput();
    }

    private void ProcessKeyboardInput()
    {
        MoveLeft = _pressedKeys.Contains(Key.Left) || _pressedKeys.Contains(Key.A);
        MoveRight = _pressedKeys.Contains(Key.Right) || _pressedKeys.Contains(Key.D);
        Fire = _pressedKeys.Contains(Key.Space) || _pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.Up);
        Pause = _pressedKeys.Contains(Key.P) || _pressedKeys.Contains(Key.Escape);
        Start = _pressedKeys.Contains(Key.Enter) || _pressedKeys.Contains(Key.Space);
    }

    private void ProcessGamepadInput()
    {
        if (!_sdlInitialized || _gameController == nint.Zero)
            return;

        try
        {
            SDL_GameControllerUpdate();

            // D-pad and left stick for movement
            bool dpadLeft = SDL_GameControllerGetButton(_gameController, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) == 1;
            bool dpadRight = SDL_GameControllerGetButton(_gameController, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) == 1;

            short leftX = SDL_GameControllerGetAxis(_gameController, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
            const short deadzone = 8000;

            MoveLeft = MoveLeft || dpadLeft || leftX < -deadzone;
            MoveRight = MoveRight || dpadRight || leftX > deadzone;

            // A button or triggers for fire
            bool aButton = SDL_GameControllerGetButton(_gameController, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) == 1;
            short rightTrigger = SDL_GameControllerGetAxis(_gameController, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);
            Fire = Fire || aButton || rightTrigger > 10000;

            // Start button
            bool startButton = SDL_GameControllerGetButton(_gameController, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) == 1;
            Start = Start || startButton;

            // Back/Select for pause
            bool backButton = SDL_GameControllerGetButton(_gameController, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) == 1;
            Pause = Pause || backButton;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gamepad read error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_gameController != nint.Zero)
        {
            try
            {
                SDL_GameControllerClose(_gameController);
            }
            catch { }
            _gameController = nint.Zero;
        }

        if (_sdlInitialized)
        {
            try
            {
                SDL_Quit();
            }
            catch { }
            _sdlInitialized = false;
        }
    }
}
