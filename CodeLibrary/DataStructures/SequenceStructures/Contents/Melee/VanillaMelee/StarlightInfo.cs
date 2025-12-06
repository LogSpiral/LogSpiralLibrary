using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 银色战车！！
/// </summary>
public class StarlightInfo : VanillaMelee
{
    #region 重写属性

    public override float OffsetRotation => Main.rand.NextFloat(-1f, 1f) * MathHelper.Pi / 12f;
    public override Vector2 OffsetCenter => (Main.rand.NextVector2Unit() * new Vector2(16, 4) + 16 * Vector2.UnitX).RotatedBy(Rotation);
    public override bool Attacktive => true;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartSingle()
    {
        Flip = Owner.direction != 1;
        ShootExtraProjectile();

        base.OnStartSingle();
    }

    public override void OnAttack()
    {
        if (IsLocalProjectile)
        {
            Vector2 tarVec = Owner switch
            {
                Player plr => Main.MouseWorld,
                _ => default
            };
            Rotation = (tarVec - Owner.Center).ToRotation();
        }

        if ((int)LogSpiralLibraryMod.ModTime % 10 == 0)
            NetUpdateNeeded = true;
        if (Timer % 3 == 0)
        {
            SoundEngine.PlaySound((StandardInfo.soundStyle ?? MySoundID.SwooshNormal_1) with { MaxInstances = -1 }, Owner?.Center);
        }
        base.OnAttack();
    }

    public override bool Collide(Rectangle rectangle)
    {
        if (Attacktive)
        {
            Projectile.localNPCHitCooldown = Math.Clamp(TimerMax / 2, 1, 514);
            float point1 = 0f;
            return Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), Projectile.Center,
                    targetedVector * 1.5f + Projectile.Center, 48f, ref point1);
        }
        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        base.Draw(spriteBatch, texture);
        float sc = 1;
        if (Owner is Player plr)
            sc = plr.GetAdjustedItemScale(plr.HeldItem);

        var hsl = Main.rgbToHsl(StandardInfo.standardColor);


        for (int n = 0; n < 3; n++) 
        {
            var nhsl = hsl;
            nhsl.X += Main.rand.NextFloat(-0.1f, 0.1f);
            nhsl.Y += .2f;
            nhsl.Y = MathHelper.Clamp(nhsl.Y, 0, 1);
            if (nhsl.X > 1)
                nhsl.X %= 1;
            if (nhsl.X < 0)
                nhsl.X += 1;
            spriteBatch.DrawStarLight(
                Rotation,
                Owner.Center,
                Main.hslToRgb(nhsl),
                ModifyData.Size * sc * OffsetSize * texture.Size().Length() * 3,
                1,
                0.5f);
        }

    }

    //public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    //{
    //    base.GetWeaponVertex
    //    Vector2 finalOrigin = offsetOrigin + standardInfo.standardOrigin;
    //    float finalRotation = offsetRotation + standardInfo.standardRotation;
    //    Vector2 drawCen = offsetCenter + Owner.Center;
    //    float sc = 1;
    //    if (Owner is Player plr)
    //        sc = plr.GetAdjustedItemScale(plr.HeldItem);
    //    return DrawingMethods.GetItemVertexes(finalOrigin, TODO, finalRotation, Rotation, texture, KValue, ModifyData.actionOffsetSize * sc, drawCen, !flip, alpha, standardInfo.frame);
    //}
    #endregion 重写函数
}