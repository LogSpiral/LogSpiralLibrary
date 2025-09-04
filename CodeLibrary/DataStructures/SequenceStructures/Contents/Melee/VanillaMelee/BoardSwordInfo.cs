using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary.Utilties;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.VanillaMelee;

/// <summary>
/// 经典宽剑
/// </summary>
public class BoardSwordInfo : VanillaMelee
{
    #region 重写属性

    public override float OffsetRotation => MathHelper.SmoothStep(0.15f, -0.75f, Factor * Factor) * MathHelper.Pi * Owner.direction;
    public override bool Attacktive => Factor < .75f;

    #endregion 重写属性

    #region 重写函数

    public override void OnStartSingle()
    {
        Flip = Owner.direction != 1;
        base.OnStartSingle();
    }

    public override void OnStartAttack()
    {
        SoundEngine.PlaySound(StandardInfo.soundStyle ?? MySoundID.Scythe, Owner?.Center);
        if (Owner is Player plr)
        {
            plr.ItemCheck_Shoot(plr.whoAmI, plr.HeldItem, CurrentDamage);
        }
        base.OnStartAttack();
    }

    public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        float origf = fTimer;
        IEnumerable<CustomVertexInfo> result = [];
        fTimer += 2.0f;
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