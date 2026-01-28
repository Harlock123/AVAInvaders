namespace AVAInvaders.Models;

public enum InvaderType
{
    Small,   // Top row - 30 points
    Medium,  // Middle rows - 20 points
    Large    // Bottom rows - 10 points
}

public class Invader : GameObject
{
    public InvaderType Type { get; }
    public int Points { get; }
    public int AnimationFrame { get; set; }

    public Invader(InvaderType type)
    {
        Type = type;
        Width = 44;
        Height = 32;

        Points = type switch
        {
            InvaderType.Small => 30,
            InvaderType.Medium => 20,
            InvaderType.Large => 10,
            _ => 10
        };
    }
}
