using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
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
    public override float offsetRotation => Main.rand.NextFloat(-1f, 1f) * MathHelper.Pi / 12f;
    public override Vector2 offsetCenter => (Main.rand.NextVector2Unit() * new Vector2(16, 4) + 16 * Vector2.UnitX).RotatedBy(Rotation);
    public override bool Attacktive => true;
    #endregion

    #region 重写函数
    public override void OnStartSingle()
    {
        flip = Owner.direction != 1;
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        base.OnStartSingle();
    }

    public override void OnAttack()
    {
        Vector2 tarVec = Owner switch
        {
            Player plr => plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition,
            _ => default
        };
        Rotation = (tarVec - Owner.Center).ToRotation();
        if (timer % 3 == 0)
        {
            SoundEngine.PlaySound((standardInfo.soundStyle ?? MySoundID.SwooshNormal_1) with { MaxInstances = -1 });
        }
        base.OnAttack();
    }

    public override bool Collide(Rectangle rectangle)
    {
        if (Attacktive)
        {
            Projectile.localNPCHitCooldown = Math.Clamp(timerMax / 2, 1, 514);
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
        spriteBatch.DrawStarLight(Rotation, Owner.Center, standardInfo.standardColor, ModifyData.actionOffsetSize * sc * offsetSize * texture.Size().Length() * 3, 1, 1f);
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
    
    #endregion
}