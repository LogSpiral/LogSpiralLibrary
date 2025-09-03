using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.Utilties;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 圣骑士会个🔨
/// </summary>
public class HammerInfo : VanillaMelee
{
    #region 重写属性

    public override float offsetRotation => Factor * MathHelper.TwoPi + (float)LogSpiralLibraryMod.ModTime2 * .025f;
    public override Vector2 offsetCenter => Rotation.ToRotationVector2() * MathF.Pow(1 - MathF.Abs(2 * (Factor * 2 % 1) - 1), 2) * 256;
    public override bool Attacktive => true;
    public override bool OwnerHitCheek => false;

    #endregion 重写属性

    #region 重写函数

    public override void UpdateStatus(bool triggered)
    {
        Timer--;
        if ((int)LogSpiralLibraryMod.ModTime2 % 6 == 0)
            SoundEngine.PlaySound(MySoundID.BoomerangRotating, Owner?.Center);
    }

    public override void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        base.OnHitEntity(victim, damageDone, context);
        if (Owner is Player plr && Main.rand.NextBool(3))
        {
            plr.Center += offsetCenter;
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
            plr.Center -= offsetCenter;
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

    #endregion 重写函数
}