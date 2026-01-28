namespace AVAInvaders.Models;

public class Player : GameObject
{
    public const double Speed = 300;
    public const double BulletCooldown = 0.35;

    public int Lives { get; set; } = 3;
    public double LastShotTime { get; set; } = -BulletCooldown;
    public bool IsExploding { get; set; }
    public double ExplosionTime { get; set; }

    public Player()
    {
        Width = 52;
        Height = 32;
    }

    public bool CanShoot(double currentTime)
    {
        return !IsExploding && currentTime - LastShotTime >= BulletCooldown;
    }
}
