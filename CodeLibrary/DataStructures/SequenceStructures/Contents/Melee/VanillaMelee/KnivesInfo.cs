using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using System.Collections.Generic;
using System.Linq;
namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;
/// <summary>
/// ─━╋ ─━╋ ─━╋
/// </summary>
public class KnivesInfo : VanillaMelee
{
    #region 重写属性
    public override float offsetRotation => base.offsetRotation + MathF.Pow(1 - Factor, 4) * MathHelper.Pi * 4;
    public override Vector2 offsetCenter => Rotation.ToRotationVector2() * (1 - Factor) * 1024 + new Vector2(0, MathF.Pow(1 - Factor, 2) * 256);
    public override bool Attacktive => true;
    #endregion

    #region 重写函数
    public override void Update(bool triggered)
    {
        Timer--;
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(10))
        {
            Vector2 orig = plr.Center;
            plr.Center = offsetCenter + Owner.Center;
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
            plr.Center = orig;
            if (Main.myPlayer == plr.whoAmI && Main.netMode == NetmodeID.MultiplayerClient)
            {
                SyncPlayerPosition.Get(plr.whoAmI, plr.position).Send(-1, plr.whoAmI);
            }
        }
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        float origf = fTimer;
        IEnumerable<CustomVertexInfo> result = [];
        fTimer += 2f;
        for (int i = 9; i >= 0; i--)
        {
            fTimer -= .2f;
            result = result.Concat(base.GetWeaponVertex(texture, (1f - i / 10f) * (i == 0 ? 1f : .5f)));
        }
        fTimer = origf;
        return [.. result];
    }
    #endregion
}