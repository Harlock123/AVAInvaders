namespace AVAInvaders.Models;

public class Bunker : GameObject
{
    public const int PixelWidth = 44;
    public const int PixelHeight = 32;

    public bool[,] Pixels { get; }

    public Bunker()
    {
        Width = PixelWidth;
        Height = PixelHeight;
        Pixels = new bool[PixelWidth, PixelHeight];
        InitializeBunkerShape();
    }

    private void InitializeBunkerShape()
    {
        // Create classic bunker shape
        for (int x = 0; x < PixelWidth; x++)
        {
            for (int y = 0; y < PixelHeight; y++)
            {
                // Top arch
                int centerX = PixelWidth / 2;
                int distFromCenter = Math.Abs(x - centerX);

                // Top curved part
                if (y < 8)
                {
                    int threshold = 8 - y;
                    Pixels[x, y] = distFromCenter <= centerX - threshold;
                }
                // Middle solid part
                else if (y < 20)
                {
                    Pixels[x, y] = true;
                }
                // Bottom part with cutout
                else
                {
                    // Create bottom arch cutout
                    int cutoutWidth = 12;
                    int cutoutStart = (PixelWidth - cutoutWidth) / 2;
                    Pixels[x, y] = x < cutoutStart || x >= cutoutStart + cutoutWidth;
                }
            }
        }
    }

    public void TakeDamage(double hitX, double hitY, int radius = 4)
    {
        int localX = (int)(hitX - X);
        int localY = (int)(hitY - Y);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = localX + dx;
                    int py = localY + dy;

                    if (px >= 0 && px < PixelWidth && py >= 0 && py < PixelHeight)
                    {
                        Pixels[px, py] = false;
                    }
                }
            }
        }
    }

    public bool CheckPixelCollision(double bulletX, double bulletY, double bulletWidth, double bulletHeight)
    {
        int startX = Math.Max(0, (int)(bulletX - X));
        int endX = Math.Min(PixelWidth, (int)(bulletX + bulletWidth - X));
        int startY = Math.Max(0, (int)(bulletY - Y));
        int endY = Math.Min(PixelHeight, (int)(bulletY + bulletHeight - Y));

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (Pixels[x, y])
                {
                    return true;
                }
            }
        }

        return false;
    }
}
