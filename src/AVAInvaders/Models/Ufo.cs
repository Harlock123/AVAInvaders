namespace AVAInvaders.Models;

public class Ufo : GameObject
{
    public const double Speed = 150;

    public int Points { get; }
    public bool MovingRight { get; }

    public Ufo(bool movingRight)
    {
        MovingRight = movingRight;
        Width = 64;
        Height = 28;

        // Random points: 50, 100, 150, or 300
        var random = new Random();
        Points = random.Next(4) switch
        {
            0 => 50,
            1 => 100,
            2 => 150,
            _ => 300
        };
    }
}
