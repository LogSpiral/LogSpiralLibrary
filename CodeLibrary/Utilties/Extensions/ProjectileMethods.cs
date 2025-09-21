using LogSpiralLibrary.CodeLibrary.Utilties.BaseClasses;
using static Terraria.Utils;

namespace LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

public static class ProjectileMethods
{
    public static bool HammerCollide(this IHammerProj hammerProj, Rectangle targetHitbox)
    {
        float point = 0f;
        var center = hammerProj.CollidingCenter;
        var origin = hammerProj.DrawOrigin;
        var projCenter = hammerProj.projCenter;
        var size = hammerProj.CollidingSize;
        var rotation = hammerProj.Rotation;
        return targetHitbox.Intersects(CenteredRectangle((center - origin).RotatedBy(rotation) + projCenter, size))
            || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projCenter, (center - origin).RotatedBy(rotation) + projCenter, 8, ref point);
    }
}