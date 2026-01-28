namespace AVAInvaders.Models;

public abstract class GameObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsAlive { get; set; } = true;

    public Rect Bounds => new(X, Y, Width, Height);

    public bool CollidesWith(GameObject other)
    {
        if (!IsAlive || !other.IsAlive)
            return false;

        return X < other.X + other.Width &&
               X + Width > other.X &&
               Y < other.Y + other.Height &&
               Y + Height > other.Y;
    }
}

public readonly record struct Rect(double X, double Y, double Width, double Height);
