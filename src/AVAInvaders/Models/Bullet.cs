namespace AVAInvaders.Models;

public class Bullet : GameObject
{
    public const double PlayerBulletSpeed = 600;
    public const double InvaderBulletSpeed = 250;

    public bool IsPlayerBullet { get; }
    public double Speed { get; }

    public Bullet(bool isPlayerBullet)
    {
        IsPlayerBullet = isPlayerBullet;
        Speed = isPlayerBullet ? PlayerBulletSpeed : InvaderBulletSpeed;
        Width = 4;
        Height = isPlayerBullet ? 14 : 20;
    }
}
