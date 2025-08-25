using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

/// <summary>
/// 原版的私有方法
/// 有几个落在绘制那边了，搬过来不太合适
/// </summary>
public static class VanillaPrivateMethod
{
    public static bool FindSharpTearsOpening(int x, int y, bool acceptLeft, bool acceptRight, bool acceptUp, bool acceptDown)
    {
        if (acceptLeft && !WorldGen.SolidTile(x - 1, y))
            return true;

        if (acceptRight && !WorldGen.SolidTile(x + 1, y))
            return true;

        if (acceptUp && !WorldGen.SolidTile(x, y - 1))
            return true;

        if (acceptDown && !WorldGen.SolidTile(x, y + 1))
            return true;

        return false;
    }

    public static Point FindSharpTearsSpot(this Player player, Vector2 targetSpot)
    {
        Point point = targetSpot.ToTileCoordinates();
        Vector2 center = player.Center;
        Vector2 endPoint = targetSpot;
        int samplesToTake = 3;
        float samplingWidth = 4f;
        Collision.AimingLaserScan(center, endPoint, samplingWidth, samplesToTake, out Vector2 vectorTowardsTarget, out float[] samples);
        float num = float.PositiveInfinity;
        for (int i = 0; i < samples.Length; i++)
        {
            if (samples[i] < num)
                num = samples[i];
        }

        targetSpot = center + vectorTowardsTarget.SafeNormalize(Vector2.Zero) * num;
        point = targetSpot.ToTileCoordinates();
        Rectangle value = new(point.X, point.Y, 1, 1);
        value.Inflate(6, 16);
        Rectangle value2 = new(0, 0, Main.maxTilesX, Main.maxTilesY);
        value2.Inflate(-40, -40);
        value = Rectangle.Intersect(value, value2);
        List<Point> list = [];
        List<Point> list2 = [];
        for (int j = value.Left; j <= value.Right; j++)
        {
            for (int k = value.Top; k <= value.Bottom; k++)
            {
                if (!WorldGen.SolidTile(j, k))
                    continue;

                Vector2 value3 = new(j * 16 + 8, k * 16 + 8);
                if (!(Vector2.Distance(targetSpot, value3) > 200f))
                {
                    if (FindSharpTearsOpening(j, k, j > point.X, j < point.X, k > point.Y, k < point.Y))
                        list.Add(new Point(j, k));
                    else
                        list2.Add(new Point(j, k));
                }
            }
        }

        if (list.Count == 0 && list2.Count == 0)
            list.Add((player.Center.ToTileCoordinates().ToVector2() + Main.rand.NextVector2Square(-2f, 2f)).ToPoint());

        List<Point> list3 = list;
        if (list3.Count == 0)
            list3 = list2;

        int index = Main.rand.Next(list3.Count);
        return list3[index];
    }
}